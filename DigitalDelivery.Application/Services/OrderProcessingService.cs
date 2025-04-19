using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Domain.Enums;
using DigitalDelivery.Infrastructure.Queues;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DigitalDelivery.Application.Services
{
    public class OrderProcessingService : BackgroundService
    {
        private readonly IOrderQueue _orderQueue;
        private readonly IRobotService _robotService;
        private readonly IOrderService _orderService;
        private readonly IRobotSelectionStrategyFactory _strategyFactory;
        private readonly ILogger<OrderProcessingService> _logger;

        public OrderProcessingService(
            IOrderQueue orderQueue,
            IRobotService robotService,
            IOrderService orderService,
            IRobotSelectionStrategyFactory strategyFactory,
            ILogger<OrderProcessingService> logger)
        {
            _orderQueue = orderQueue;
            _robotService = robotService;
            _orderService = orderService;
            _strategyFactory = strategyFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Order processing service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var order = await _orderQueue.DequeueAsync();

                if (order != null)
                {
                    _logger.LogInformation($"Processing order {order.Id}");

                    await ProcessOrderAsync(order);
                    await Task.Delay(1000);

                    _logger.LogInformation($"Order {order.Id} completed.");
                }
                else
                {
                    await Task.Delay(500);
                }
            }

            _logger.LogInformation("Order processing service stopped.");
        }

        private async Task ProcessOrderAsync(Order order)
        {
            await _orderService.ChangeStatusAsync(order.Id, OrderStatusEnum.Processing);

            var availableRobot = _robotService.GetAvailableRobotsQuery(order.PackageDetails);

            var strategy = _strategyFactory.GetStrategy(order);
            var robot = strategy.SelectBestRobot(availableRobot, order);
        }
    }
}