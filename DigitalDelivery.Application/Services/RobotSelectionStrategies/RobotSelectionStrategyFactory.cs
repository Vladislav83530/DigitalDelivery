using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDelivery.Application.Services.RobotSelectionStrategies
{
    public class RobotSelectionStrategyFactory : IRobotSelectionStrategyFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public RobotSelectionStrategyFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IRobotSelectionStrategy GetStrategy(Order order)
        {
            return _serviceProvider.GetRequiredService<IRobotSelectionStrategy>();
        }
    }
}
