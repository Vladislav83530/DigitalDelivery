using DigitalDelivery.Domain.Entities;

namespace DigitalDelivery.Application.Interfaces
{
    public interface IRobotSelectionStrategyFactory
    {
        IRobotSelectionStrategy GetStrategy(Order order);
    }
}
