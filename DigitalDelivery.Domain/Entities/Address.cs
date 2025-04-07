using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalDelivery.Domain.Entities
{
    [Table("addresses")]
    public class Address
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("latitude")]
        public double Latitude { get; set; }

        [Column("longitude")]
        public double Longitude { get; set; }

        public ICollection<Order> PickupOrders { get; set; }
        public ICollection<Order> DeliveryOrders { get; set; }
    }
}
