using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalDelivery.Domain.Entities
{
    [Table("robots")]
    public class Robot
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("model")]
        public string Model { get; set; }

        public RobotSpecification Specification { get; set; }
        public RobotTelemetry Telemetry { get; set; }
        public ICollection<RobotAssignment> Assignments { get; set; }
    }
}