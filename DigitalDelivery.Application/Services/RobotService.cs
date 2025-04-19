using DigitalDelivery.Application.Helpers;
using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Domain.Enums;
using DigitalDelivery.Infrastructure.EF;

namespace DigitalDelivery.Application.Services
{
    public class RobotService : IRobotService
    {
        private readonly AppDbContext _context;

        public RobotService(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Robot> GetAvailableRobotsQuery(PackageDetails packageDetails)
        {
            var availableStatuses = new List<RobotStatus> { RobotStatus.Available, RobotStatus.Busy, RobotStatus.Charging };

            var availableRobots = _context.Robots
                .Where(r => 
                    r.Specification.LoadCapacityKg >= packageDetails.WeightKg &&
                    availableStatuses.Contains(r.Telemetry.Status) &&
                    DeliveryHelper.CanFitPackage(packageDetails, r)
                );

            return availableRobots;
        }
    }
}