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

        [Column("date_in")]
        public DateTime DateIn { get; set; }

        [Column("order_id")]
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        public Order Order { get; set; }
    }
}