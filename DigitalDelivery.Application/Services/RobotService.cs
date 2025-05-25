using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Application.Models;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Domain.Enums;
using DigitalDelivery.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;

namespace DigitalDelivery.Application.Services
{
    public class RobotService : IRobotService
    {
        private readonly AppDbContext _context;
        private readonly ISimulatedClock _clock;

        public RobotService(AppDbContext context, ISimulatedClock clock)
        {
            _context = context;
            _clock = clock;

        }

        public Task<Node> GetCurrentLocationAsync(Guid robotId)
        {
            return _context.Robots
                .Where(r => r.Id == robotId)
                .Select(r => new Node
                {
                    Latitude = r.Telemetry.Latitude,
                    Longitude = r.Telemetry.Longitude
                })
                .FirstOrDefaultAsync();
        }

        public IQueryable<Robot> GetAvailableRobotsQuery(PackageDetails packageDetails)
        {
            var availableStatuses = new List<RobotStatus> { RobotStatus.Available };

            var availableRobots = _context.Robots
                .Include(r => r.Specification)
                .Include(r => r.Telemetry)
                .Where(r =>
                    r.Specification.LoadCapacityKg >= packageDetails.WeightKg &&
                    r.Telemetry.Status == RobotStatus.Available &&
                    r.Assignments.All(a => a.Order.OrderStatuses
                        .OrderByDescending(o => o.DateIn)
                        .FirstOrDefault().Status != OrderStatusEnum.Processed)
                );

            return availableRobots;
        }

        public async Task<Result<string>> AssignTaskAsync(Guid robotId, int orderId)
        {
            _context.RobotAssignments
                .Add(new RobotAssignment
                {
                    RobotId = robotId,
                    OrderId = orderId,
                    AssignmentAt = _clock.Now
                });

            await _context.SaveChangesAsync();

            return new Result<string>(true);
        }

        public async Task<Result<string>> ChangeStatusAsync(Guid id, RobotStatus status)
        {
            var robot = _context.Robots
                .Include(r => r.Telemetry)
                .FirstOrDefault(o => o.Id == id);

            if (robot == null)
            {
                return new Result<string>(false, "Robot not found.");
            }

            robot.Telemetry.LastUpdate = _clock.Now;
            robot.Telemetry.Status = status;
            await _context.SaveChangesAsync();

            return new Result<string>(true);
        }

        public async Task<RobotStatus> GetRobotStatusAsync(Guid id)
        {
            return await _context.Robots
                .Where(r => r.Id == id)
                .Select(r => r.Telemetry.Status)
                .FirstOrDefaultAsync();
        }

        public async Task<Result<string>> SaveRobotDataAsync(Robot robot)
        {
            if (robot == null)
            {
                return new Result<string>(false, "Robot is null");
            }
            
            _context.Robots.Update(robot);
            
            await _context.SaveChangesAsync();
            
            return new Result<string>(true, "Robot data saved successfully");
        }

        public async Task<List<Robot>> GetAllChargingRobotAsync()
        {
            return await _context.Robots
                .Include(r => r.Telemetry)
                .Where(r => r.Telemetry.Status == RobotStatus.Charging)
                .ToListAsync();
        }

        public async Task<Result<string>> SetBatteryLevelAsync(Guid id, double batteryLevel)
        {
            var robot = _context.Robots
                .Include(r => r.Telemetry)
                .FirstOrDefault(o => o.Id == id);

            if (robot == null)
            {
                return new Result<string>(false, "Robot not found.");
            }

            robot.Telemetry.BatteryLevel = batteryLevel;
            await _context.SaveChangesAsync();

            return new Result<string>(true);
        }
    }
}