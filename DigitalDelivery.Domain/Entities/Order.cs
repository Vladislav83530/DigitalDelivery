using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalDelivery.Domain.Entities
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [ForeignKey("User")]
        [Column("sender_id")]
        public int SenderId { get; set; }
        public User Sender { get; set; }

        [ForeignKey("User")]
        [Column("recipient_id")]
        public int RecipientId { get; set; }
        public User Recipient { get; set; }

        [ForeignKey("Address")]
        [Column("pickup_address_id")]
        public int PickupAddressId { get; set; }
        public Address PickupAddress { get; set; }

        [ForeignKey("Address")]
        [Column("delivery_address_id")]
        public int DeliveryAddressId { get; set; }
        public Address DeliveryAddress { get; set; }

        [ForeignKey("OrderStatus")]
        [Column("status_id")]
        public int StatusId { get; set; }
        public OrderStatus Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("estimated_delivery")]
        public DateTime? EstimatedDelivery { get; set; }

        [Column("cost")]
        public double Cost { get; set; }

        public PackageDetails PackageDetails { get; set; }
    }
}