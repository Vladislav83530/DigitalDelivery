using DigitalDelivery.Domain.Enums;

namespace DigitalDelivery.Application.Models.Order
{
    public class OrderHistoryItem
    {
        public int OrderNumber { get; set; }
        public OrderStatusEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }
}
