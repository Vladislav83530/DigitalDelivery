using DigitalDelivery.Domain.Entities;

namespace DigitalDelivery.Application.Models.Order
{
    public class ContactViewModel
    {
        public Node Address { get; set; }

        public string PhoneNumber { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
