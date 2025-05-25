using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DigitalDelivery.Domain.Entities
{
    [Table("edges")]
    public class Edge
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("from_node_id")]
        public long FromNodeId { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(FromNodeId))]
        public Node FromNode { get; set; }

        [Column("to_node_id")]
        public long ToNodeId { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(ToNodeId))]
        public Node ToNode { get; set; }

        [Column("cost")]
        public double Cost { get; set; }
    }
}