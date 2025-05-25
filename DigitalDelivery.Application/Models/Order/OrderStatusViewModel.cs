using DigitalDelivery.Domain.Enums;

namespace DigitalDelivery.Application.Models.Order
{
    public class OrderStatusViewModel
    {
        public OrderStatusEnum Status { get; set; }

        public DateTime DateIn { get; set; }
    }
}
