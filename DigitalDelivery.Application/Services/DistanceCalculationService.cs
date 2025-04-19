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
            double distance1 = CalculateDistanceBetweenPoints(robotLocation, pickupAddress);
            double distance2 = CalculateDistanceBetweenPoints(pickupAddress, deliveryAddress);
            double distance3 = CalculateDistanceBetweenPoints(deliveryAddress, _mapSettings.ChargingStation);

            double minDistance = distance1 + distance2;
            double minDistanceWithCharging = minDistance + distance3;
            return (minDistance, minDistanceWithCharging);
        }

        private double CalculateDistanceBetweenPoints(GeoCoordinate point1, GeoCoordinate point2)
        {
            double lat1 = point1.Latitude * Math.PI / 180;
            double lon1 = point1.Longitude * Math.PI / 180;
            double lat2 = point2.Latitude * Math.PI / 180;
            double lon2 = point2.Longitude * Math.PI / 180;

            double dlon = lon2 - lon1;
            double dlat = lat2 - lat1;

            double a = Math.Pow(Math.Sin(dlat / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dlon / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double earthRadius = 6371;
            double distance = earthRadius * c;

            return distance * 1000;
        }
    }
}
