using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Domain.Entities;
using DigitalDelivery.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace DigitalDelivery.Application.Services
{
    public class RobotSimulationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RobotSimulationService> _logger;
        private readonly ISimulatedClock _simulatedClock;

        public RobotSimulationService(
            ILogger<RobotSimulationService> logger,
            IServiceScopeFactory scopeFactory,
            ISimulatedClock simulatedClock)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _simulatedClock = simulatedClock;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Running scheduled job at: {time}", _simulatedClock.Now);


                using var scope = _scopeFactory.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                var robotService = scope.ServiceProvider.GetRequiredService<IRobotService>();

                _simulatedClock.Pause();
                await ProcessOrdersAsync(orderService, robotService);
                _simulatedClock.Resume();

                await _simulatedClock.DelayAsync(TimeSpan.FromSeconds(20), stoppingToken);
            }
        }

        private async Task ProcessOrdersAsync(IOrderService orderService, IRobotService robotService)
        {
            var orders = await orderService.GetAllActiveOrdersAsync();

            foreach (var order in orders)
            {
                var status = order.OrderStatuses.OrderByDescending(x => x.DateIn).FirstOrDefault().Status;
                var robotStatus = await robotService.GetRobotStatusAsync(order.RobotAssignments.RobotId);

                if (status == OrderStatusEnum.Processed && robotStatus == RobotStatus.Busy)
                {
                    continue;
                }
                else if (status == OrderStatusEnum.Processed && robotStatus != RobotStatus.Busy)
                {
                    await robotService.ChangeStatusAsync(order.RobotAssignments.RobotId, RobotStatus.Busy);
                    await orderService.ChangeStatusAsync(order.Id, OrderStatusEnum.MoveToPickupPoint);
                }

                await MoveRobotAsync(orderService, robotService, order);
            }

            await CheckChargingAsync(robotService);
        }

        private async Task MoveRobotAsync(IOrderService orderService, IRobotService robotService, Order order)
        {
            var robot = order.RobotAssignments.Robot;
            var currentStatus = order.OrderStatuses.OrderByDescending(x => x.DateIn).FirstOrDefault();

            if (currentStatus == null)
                return;

            switch (currentStatus.Status)
            {
                case OrderStatusEnum.MoveToPickupPoint:
                    await HandleMoveToPickupPointAsync(orderService, robotService, order, robot, currentStatus);
                    break;

                case OrderStatusEnum.MoveToDeliveryPoint:
                    await HandleMoveToDeliveryPointAsync(orderService, robotService, order, robot, currentStatus);
                    break;
            }
        }

        private async Task HandleMoveToPickupPointAsync(
            IOrderService orderService,
            IRobotService robotService,
            Order order,
            Robot robot,
            OrderStatus currentStatus)
        {
            var route = order.Routes.FirstOrDefault(r => r.RouteType == RouteType.ToClient);
            if (route == null)
                return;

            var estimatedHours = GetEstimatedHours(route.TotalDistance, robot.Telemetry.SpeedKhp);

            if (_simulatedClock.Now >= currentStatus.DateIn.ToUniversalTime().AddHours(estimatedHours))
            {
                robot.Telemetry.Latitude = order.PickupAddress.Latitude;
                robot.Telemetry.Longitude = order.PickupAddress.Longitude;
                robot.Telemetry.BatteryLevel -= route.TotalDistance * robot.Specification.EnergyConsumptionPerM;

                await robotService.SaveRobotDataAsync(robot);
                await orderService.ChangeStatusAsync(order.Id, OrderStatusEnum.MoveToDeliveryPoint);
            }
        }

        private async Task HandleMoveToDeliveryPointAsync(
            IOrderService orderService,
            IRobotService robotService,
            Order order,
            Robot robot,
            OrderStatus currentStatus)
        {
            var route = order.Routes.FirstOrDefault(r => r.RouteType == RouteType.Delivery);
            if (route == null)
                return;

            var estimatedHours = GetEstimatedHours(route.TotalDistance, robot.Telemetry.SpeedKhp);

            if (_simulatedClock.Now >= currentStatus.DateIn.ToUniversalTime().AddHours(estimatedHours))
            {
                robot.Telemetry.Latitude = order.DeliveryAddress.Latitude;
                robot.Telemetry.Longitude = order.DeliveryAddress.Longitude;
                robot.Telemetry.BatteryLevel -= route.TotalDistance * robot.Specification.EnergyConsumptionPerM;

                await robotService.SaveRobotDataAsync(robot);
                await robotService.ChangeStatusAsync(order.RobotAssignments.RobotId, RobotStatus.Available);
                await orderService.ChangeStatusAsync(order.Id, OrderStatusEnum.Delivered);

                if (robot.Telemetry.BatteryLevel < 20)
                {
                    await robotService.ChangeStatusAsync(order.RobotAssignments.RobotId, RobotStatus.Charging);
                }
            }
        }

        private double GetEstimatedHours(double distanceMeters, double speedKph)
        {
            const double waitingBuffer = 0.1; // hours
            return (distanceMeters / 1000.0) / speedKph + waitingBuffer;
        }

        private async Task CheckChargingAsync(IRobotService robotService)
        {
            var robots = await robotService.GetAllChargingRobotAsync();

            foreach (var robot in robots)
            {
                var chargingTime = robot.Telemetry.LastUpdate - _simulatedClock.Now;

                if (chargingTime >= TimeSpan.FromMinutes(90))
                {
                    await robotService.ChangeStatusAsync(robot.Id, RobotStatus.Available);
                    await robotService.SetBatteryLevelAsync(robot.Id, 100);
                }
            }
        }
    }
}
