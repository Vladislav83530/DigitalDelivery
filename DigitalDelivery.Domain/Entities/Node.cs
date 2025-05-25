using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DigitalDelivery.Domain.Entities
{
    [Table("nodes")]
    public class Node
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("latitude")]
        public double Latitude { get; set; }

        [Column("longitude")]
        public double Longitude { get; set; }

        [Column("is_building_center")]
        public bool IsBuildingCenter { get; set; } = false;

        [JsonIgnore]
        public ICollection<Edge> OutgoingEdges { get; set; }

        [JsonIgnore]
        public ICollection<Edge> IncomingEdges { get; set; }

        [JsonIgnore]
        public ICollection<RouteNode> RouteNodes { get; set; }
    }
}
