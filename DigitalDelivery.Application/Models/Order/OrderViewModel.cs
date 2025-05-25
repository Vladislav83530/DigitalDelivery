namespace DigitalDelivery.Application.Models.Order
{
    public class OrderViewModel
    {
        public int OrderNumber { get; set; }

        public DateTime? EstimatedDelivery { get; set; }

        public PackageViewModel Package { get; set; }

        public ContactViewModel Sender { get; set; }

        public ContactViewModel Recipient { get; set; }

        public List<OrderStatusViewModel> OrderStatuses { get; set; }
    }
}