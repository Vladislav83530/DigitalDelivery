using System.Text.RegularExpressions;

namespace DigitalDelivery.Application.Helpers
{
    public static class Helper
    {
        public static string CleanPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return string.Empty;
            }

            return Regex.Replace(phoneNumber, @"\D", "");
        }

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
    }
}