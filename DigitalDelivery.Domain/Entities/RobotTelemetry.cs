using DigitalDelivery.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalDelivery.Domain.Entities
{
    [Table("robot_telemetries")]
    public class RobotTelemetry
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [ForeignKey("Robot")]
        [Column("robot_id")]
        public Guid RobotId { get; set; }
        public Robot Robot { get; set; }

        [Column("latitude")]
        public double Latitude { get; set; }

        [Column("longitude")]
        public double Longitude { get; set; }

        [Column("battery_level")]
        public double BatteryLevel { get; set; }

        [Column("speed_khp")]
        public double SpeedKhp { get; set; }

        [Column("last_update")]
        public DateTime LastUpdate { get; set; }

        [Column("status")]
        public RobotStatus Status { get; set; }
    }
}