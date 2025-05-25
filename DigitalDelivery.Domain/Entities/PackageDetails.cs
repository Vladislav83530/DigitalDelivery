using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalDelivery.Domain.Entities
{
    [Table("package_details")]
    public class PackageDetails
    {
        [Key]
        [ForeignKey("Order")]
        [Column("order_id")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [Column("weight_kg")]
        public double WeightKg { get; set; }

        [Column("width_cm")]
        public double WidthCm { get; set; }

        [Column("height_cm")]
        public double HeightCm { get; set; }

        [Column("depth_cm")]
        public double DepthCm { get; set; }
    }
}
