using DigitalDelivery.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalDelivery.Infrastructure.EF
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}