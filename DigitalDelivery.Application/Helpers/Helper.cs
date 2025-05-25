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
    }
}