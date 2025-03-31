using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalDelivery.Domain.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("firstname")]
        public string FirstName { get; set; }

        [Column("lastname")]
        public string LastName { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("token")]
        public string Token { get; set; }

        [Column("refreshtoken")]
        public string RefreshToken { get; set; }

        [Column("refreshtokenexpirytime")]
        public DateTime RefreshTokenExpiryTime { get; set; }

        public ICollection<Order> SentOrders { get; set; }
        public ICollection<Order> ReceivedOrders { get; set; }
    }
}