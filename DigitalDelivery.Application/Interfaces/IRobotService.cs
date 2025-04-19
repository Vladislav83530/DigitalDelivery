using DigitalDelivery.Domain.Entities;

namespace DigitalDelivery.Application.Interfaces
{
    public interface IRobotService
    {
        IQueryable<Robot> GetAvailableRobotsQuery(PackageDetails packageDetails);
    }
}