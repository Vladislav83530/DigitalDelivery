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
    }
}
