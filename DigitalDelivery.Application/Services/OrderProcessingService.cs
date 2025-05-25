using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Application.Models.Map;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Domain.Enums;
using DigitalDelivery.Infrastructure.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DigitalDelivery.Application.Services
{
    public class OrderProcessingService : BackgroundService
    {
        private readonly IOrderQueue _orderQueue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderProcessingService> _logger;
        private readonly ISimulatedClock _clock;

        public OrderProcessingService(
            IOrderQueue orderQueue,
            IServiceScopeFactory scopeFactory,
            ILogger<OrderProcessingService> logger,
            ISimulatedClock clock)
        {
            _orderQueue = orderQueue;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _clock = clock;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Order processing service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var order = await _orderQueue.DequeueAsync();

                if (order != null)
                {
                    //_logger.LogInformation($"Processing order {order.Id}");

                    using var scope = _scopeFactory.CreateScope();
                    var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                    var robotService = scope.ServiceProvider.GetRequiredService<IRobotService>();
                    var strategyFactory = scope.ServiceProvider.GetRequiredService<IRobotSelectionStrategyFactory>();
                    var routeService = scope.ServiceProvider.GetRequiredService<IRouteService>();

                    await ProcessOrderAsync(order, orderService, robotService, routeService, strategyFactory);

                    await _clock.DelayAsync(TimeSpan.FromSeconds(1), stoppingToken);

                    //_logger.LogInformation($"Order {order.Id} completed.");
                }
                else
                {
                    await _clock.DelayAsync(TimeSpan.FromSeconds(0.5), stoppingToken);
                }
            }

            _logger.LogInformation("Order processing service stopped.");
        }

        private async Task ProcessOrderAsync(
            Order order,
            IOrderService orderService,
            IRobotService robotService,
            IRouteService routeService,
            IRobotSelectionStrategyFactory strategyFactory)
        {
            _clock.Pause();
            await orderService.ChangeStatusAsync(order.Id, OrderStatusEnum.Processing);
            var availableRobot = robotService.GetAvailableRobotsQuery(order.PackageDetails);

            var strategy = strategyFactory.GetStrategy(order);
            var robot = strategy.SelectBestRobot(availableRobot, order);

            if (robot == null)
            {
                await _orderQueue.EnqueueAsync(order);
                return;
            }

            await SelectAndSaveRoutesAsync(routeService, orderService, order, robot);
            await robotService.AssignTaskAsync(robot.Id, order.Id);
            _clock.Resume();

            _clock.Pause();
            await orderService.ChangeStatusAsync(order.Id, OrderStatusEnum.Processed);
            _clock.Resume();
        }

        private async Task SelectAndSaveRoutesAsync(IRouteService routeService, IOrderService orderService, Order order, Robot robot)
        {
            var robotCoordinate = new GeoCoordinate(robot.Telemetry.Latitude, robot.Telemetry.Longitude);
            var pickupCoordinate = new GeoCoordinate(order.PickupAddress.Latitude, order.PickupAddress.Longitude);
            var deliveryCoordinate = new GeoCoordinate(order.DeliveryAddress.Latitude, order.DeliveryAddress.Longitude);

            var deliveryRoute = routeService.FindShortestPath(pickupCoordinate, deliveryCoordinate);
            var routeToClient = routeService.FindShortestPath(robotCoordinate, pickupCoordinate);

            var deliveryRouteDistance = await routeService.CalculateTotalDistanceAsync(deliveryRoute);
            var toClientRouteDistance = await routeService.CalculateTotalDistanceAsync(routeToClient);

            await routeService.SaveShortestPathAsync(routeToClient, RouteType.ToClient, order.Id, toClientRouteDistance);
            await routeService.SaveShortestPathAsync(deliveryRoute, RouteType.Delivery, order.Id, deliveryRouteDistance);

            await orderService.SetDeliveryAstimateAsync(new List<double> { deliveryRouteDistance, toClientRouteDistance }, robot.Id, order.Id);
        }
    }
}