using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Application.Settings;
using DigitalDelivery.Domain.Entities;
using Microsoft.Extensions.Options;
using GeoCoordinate = DigitalDelivery.Application.Models.Map.GeoCoordinate;

namespace DigitalDelivery.Application.Services.RobotSelectionStrategies
{
    public class FastestAvailableRobotStrategy : IRobotSelectionStrategy
    {
        private readonly IDistanceCalculationService _distanceCalculationService;
        private readonly BaseDeliverySettings _baseDeliverySettings;

        public FastestAvailableRobotStrategy(
            IDistanceCalculationService distanceCalculationService,
            IOptions<BaseDeliverySettings> baseDeliverySettings)
        {
            _distanceCalculationService = distanceCalculationService;
            _baseDeliverySettings = baseDeliverySettings.Value;
        }

        public Robot SelectBestRobot(IQueryable<Robot> robots, Order order)
        {
            var deliveryEstimates = new Dictionary<Robot, double>();

            foreach (var robot in robots)
            {
                (double minDistance, double minDistanceWithCharging) = GetMinDistance(robot, order);

                if (!HasEnoughtBattery(minDistanceWithCharging, robot))
                    continue;

                double baseDeliveryTime = EstimateDeliveryTime(minDistance, robot);
                double totalEstimatedTime;

                if (robot.Telemetry.Status == Domain.Enums.RobotStatus.Available)
                {
                    totalEstimatedTime = baseDeliveryTime;
                }
                else
                {
                    var nextAssignment = robot.Assignments
                        .Where(a => a.Order?.EstimatedDelivery != null)
                        .OrderByDescending(a => a.Order.EstimatedDelivery)
                        .FirstOrDefault();

                    if (nextAssignment?.Order?.EstimatedDelivery == null)
                        continue;

                    var secondsUntilFree = (nextAssignment.Order.EstimatedDelivery.Value - DateTime.UtcNow).TotalSeconds;
                    if (secondsUntilFree < 0)
                        secondsUntilFree = 0;

                    double hoursUntilFree = secondsUntilFree / 3600.0;
                    totalEstimatedTime = hoursUntilFree + baseDeliveryTime;
                }

                deliveryEstimates[robot] = totalEstimatedTime;
            }

            return deliveryEstimates.OrderBy(r => r.Value).FirstOrDefault().Key;
        }

        private bool HasEnoughtBattery(double minDistance, Robot robot)
        {
            var neededBatery = minDistance * robot.Specification.EnergyConsumptionPerM;
            var currentBateryCapacityAh = robot.Specification.BatteryCapacityAh * robot.Telemetry.BatteryLevel / 100;
            return neededBatery <= currentBateryCapacityAh;
        }

        private (double, double) GetMinDistance(Robot robot, Order order, bool withCharging = false)
        {
            var robotLocation = new GeoCoordinate(robot.Telemetry.Latitude, robot.Telemetry.Longitude);
            var pickupAddress = new GeoCoordinate(order.PickupAddress.Latitude, order.PickupAddress.Longitude);
            var deliveryAddress = new GeoCoordinate(order.DeliveryAddress.Latitude, order.DeliveryAddress.Longitude);
            var distance = _distanceCalculationService.SimpleCalculationDistance(robotLocation, pickupAddress, deliveryAddress);

            return distance;
        }

        private double EstimateDeliveryTime(double minDistance, Robot robot)
        {
            double distanceInKilometers = minDistance / 1000.0;

            double timeInHours = distanceInKilometers / robot.Specification.MaxSpeedKph;
            return timeInHours + _baseDeliverySettings.EstimateLoadingUnloadingTime;
        }
    }
}