using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalDelivery.Domain.Entities
{
    [Table("robot_specifications")]
    public class RobotSpecification
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [ForeignKey("Robot")]
        [Column("robot_id")]
        public Guid RobotId { get; set; }
        public Robot Robot { get; set; }

        [Column("load_capacity_kg")]
        public double LoadCapacityKg { get; set; }

        [Column("width")]
        public double MaxWidthCm { get; set; }

        [Column("height")]
        public double MaxHeightCm { get; set; }

        [Column("depth")]
        public double MaxDepthCm { get; set; }

        [Column("max_speed_kph")]
        public double MaxSpeedKph { get; set; }

        [Column("battery_capacity_ah")]
        public double BatteryCapacityAh { get; set; }

        [Column("energy_consumption_per_m")]
        public double EnergyConsumptionPerM { get; set; }
    }
}
