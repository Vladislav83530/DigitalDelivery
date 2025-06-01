using DigitalDelivery.Application.Models;
using DigitalDelivery.Application.Services;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Domain.Enums;

namespace DigitalDelivery.Application.Interfaces
{
    public interface IRobotService
    {
        Task<Node> GetCurrentLocationAsync(Guid robotId);
        IQueryable<Robot> GetAvailableRobotsQuery(PackageDetails packageDetails);
        Task<Result<string>> AssignTaskAsync(Guid robotId, int orderId);
        Task<Result<string>> ChangeStatusAsync(Guid id, RobotStatus status);
        Task<RobotStatus> GetRobotStatusAsync(Guid id);
        Task<Result<string>> SaveRobotDataAsync(Robot robot);
        Task<List<Robot>> GetAllChargingRobotAsync();
        Task<Result<string>> SetBatteryLevelAsync(Guid id, double batteryLevel);
    }
}