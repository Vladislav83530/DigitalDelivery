using DigitalDelivery.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalDelivery.Domain.Entities
{
    [Table("order_statuses")]
    public class OrderStatus
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("status")]
        public OrderStatusEnum Status { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}