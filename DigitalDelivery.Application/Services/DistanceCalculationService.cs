using DigitalDelivery.Application.Helpers;
using DigitalDelivery.Application.Interfaces;
using DigitalDelivery.Application.Models.Map;
using DigitalDelivery.Application.Settings;
using Microsoft.Extensions.Options;

namespace DigitalDelivery.Application.Services
{
    public class DistanceCalculationService : IDistanceCalculationService
    {
        private readonly MapSettings _mapSettings;

        public DistanceCalculationService(IOptions<MapSettings> mapSettings)
        {
            _mapSettings = mapSettings.Value;
        }

        public (double, double) SimpleCalculationDistance(GeoCoordinate robotLocation, GeoCoordinate pickupAddress, GeoCoordinate deliveryAddress)
        {
            double distance1 = DeliveryHelper.CalculateDistanceBetweenPoints(robotLocation, pickupAddress);
            double distance2 = DeliveryHelper.CalculateDistanceBetweenPoints(pickupAddress, deliveryAddress);
            double distance3 = DeliveryHelper.CalculateDistanceBetweenPoints(deliveryAddress, _mapSettings.ChargingStation);

            double minDistance = distance1 + distance2;
            double minDistanceWithCharging = minDistance + distance3;
            return (minDistance, minDistanceWithCharging);
        }


    }
}
