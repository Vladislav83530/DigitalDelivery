using DigitalDelivery.Application.Models.Map;
using DigitalDelivery.Domain.Entities;

namespace DigitalDelivery.Application.Helpers
{
    public static class DeliveryHelper
    {
        public static bool IsPointInPolygon(double latitude, double longitude, List<(double Lat, double Lon)> polygon)
        {
            int count = polygon.Count;
            bool inside = false;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                var pi = polygon[i];
                var pj = polygon[j];

                bool intersects = ((pi.Lon > longitude) != (pj.Lon > longitude)) &&
                                  (latitude < (pj.Lat - pi.Lat) * (longitude - pi.Lon) / (pj.Lon - pi.Lon + double.Epsilon) + pi.Lat);

                if (intersects)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        public static bool CanFitPackage(PackageDetails package, Robot robot)
        {
            double packageVolume = package.WidthCm * package.HeightCm * package.DepthCm;
            double robotVolume = robot.Specification.MaxWidthCm * robot.Specification.MaxHeightCm * robot.Specification.MaxDepthCm;

            if (packageVolume > robotVolume)
            {
                return false;
            }

            var orientations = new List<(double width, double height, double depth)>
            {
                (package.WidthCm, package.HeightCm, package.DepthCm),
                (package.HeightCm, package.WidthCm, package.DepthCm),
                (package.DepthCm, package.WidthCm, package.HeightCm)
            };

            foreach (var orientation in orientations)
            {
                if (orientation.width <= robot.Specification.MaxWidthCm &&
                    orientation.height <= robot.Specification.MaxHeightCm &&
                    orientation.depth <= robot.Specification.MaxDepthCm)
                {
                    return true;
                }
            }

            return false;
        }

        public static double CalculateDistanceBetweenPoints(GeoCoordinate point1, GeoCoordinate point2)
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
