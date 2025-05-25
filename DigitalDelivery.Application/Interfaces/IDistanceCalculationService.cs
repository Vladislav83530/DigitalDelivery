using DigitalDelivery.Application.Models.Map;

namespace DigitalDelivery.Application.Interfaces
{
    public interface IDistanceCalculationService
    {
        (double, double) SimpleCalculationDistance(GeoCoordinate robotLocation, GeoCoordinate pickupAddress, GeoCoordinate deliveryAddress);
    }
}