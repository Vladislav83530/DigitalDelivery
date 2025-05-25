using DigitalDelivery.Domain.Entities;

namespace DigitalDelivery.Application.Interfaces
{
    public interface IRobotSelectionStrategy
    {
        Robot SelectBestRobot(IQueryable<Robot> robots, Order order);
    }
}