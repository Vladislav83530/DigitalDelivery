using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DigitalDelivery.Domain.Entities
{
    [Table("route_nodes")]
    public class RouteNode
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("route_id")]
        public int RouteId { get; set; }

        [JsonIgnore]
        public Route Route { get; set; }

        [Column("node_id")]
        public long NodeId { get; set; }

        [JsonIgnore]
        public Node Node { get; set; }
    }
}
