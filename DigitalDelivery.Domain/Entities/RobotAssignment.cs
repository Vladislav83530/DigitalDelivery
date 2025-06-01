using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalDelivery.Domain.Entities
{
    [Table("robot_assignments")]
    public class RobotAssignment
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [ForeignKey("Robot")]
        [Column("robot_id")]
        public Guid RobotId { get; set; }
        public Robot Robot { get; set; }

        [ForeignKey("Order")]
        [Column("order_id")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [Column("assignment_at")]
        public DateTime AssignmentAt { get; set; }

        [Column("waiting_time")]
        public int WaitingTime { get; set; }
    }
}
