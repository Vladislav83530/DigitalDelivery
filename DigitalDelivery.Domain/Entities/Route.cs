using DigitalDelivery.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DigitalDelivery.Domain.Entities
{
    [Table("routes")]
    public class Route
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("start_node_id")]
        public long StartNodeId { get; set; }

        [Column("end_node_id")]
        public long EndNodeId { get; set; }

        [Column("total_distance")]
        public double TotalDistance { get; set; }

        [Column("create_at")]
        public DateTime CreatedAt { get; set; }

        [Column("route_type")]
        public RouteType RouteType { get; set; }

        [ForeignKey("Order")]
        [Column("order_id")]
        public int? OrderId { get; set; }

        [JsonIgnore]
        public Order Order { get; set; }

        [JsonIgnore]
        public ICollection<RouteNode> RouteNodes { get; set; }
    }
}
