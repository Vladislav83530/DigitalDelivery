using DigitalDelivery.Application.Models.Map;

namespace DigitalDelivery.Application.Settings
{
    public class MapSettings
    {
        public List<GeoCoordinate> AreaPolygonCoordinates { get; set; }
        public GeoCoordinate ChargingStation { get; set; }
    }
}