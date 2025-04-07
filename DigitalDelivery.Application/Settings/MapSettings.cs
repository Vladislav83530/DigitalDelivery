namespace DigitalDelivery.Application.Settings
{
    public class MapSettings
    {
        public List<GeoCoordinate> AreaPolygonCoordinates { get; set; }
    }

    public class GeoCoordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}