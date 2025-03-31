using DigitalDelivery.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalDelivery.Infrastructure.EF
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<PackageDetails> PackageDetails { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Sender)
                .WithMany(u => u.SentOrders)
                .HasForeignKey(o => o.SenderId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Recipient)
                .WithMany(u => u.ReceivedOrders)
                .HasForeignKey(o => o.RecipientId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.PickupAddress)
                .WithMany(a => a.PickupOrders)
                .HasForeignKey(o => o.PickupAddressId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.DeliveryAddress)
                .WithMany(a => a.DeliveryOrders)
                .HasForeignKey(o => o.DeliveryAddressId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Status)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.StatusId);

            modelBuilder.Entity<PackageDetails>()
                .HasOne(pd => pd.Order)
                .WithOne(o => o.PackageDetails)
                .HasForeignKey<PackageDetails>(pd => pd.OrderId);
        }
    }
}