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
        public DbSet<Robot> Robots { get; set; }
        public DbSet<RobotAssignment> RobotAssignments { get; set; }
        public DbSet<RobotSpecification> RobotSpecifications { get; set; }
        public DbSet<RobotTelemetry> RobotTelemetries { get; set; }
        public DbSet<Node> Nodes { get; set; }
        public DbSet<Edge> Edges { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<RouteNode> RouteNodes { get; set; }

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
                .HasMany(o => o.OrderStatuses)
                .WithOne(s => s.Order)
                .HasForeignKey(o => o.OrderId);

            modelBuilder.Entity<PackageDetails>()
                .HasOne(pd => pd.Order)
                .WithOne(o => o.PackageDetails)
                .HasForeignKey<PackageDetails>(pd => pd.OrderId);

            modelBuilder.Entity<RobotAssignment>()
                .HasOne(ra => ra.Robot)
                .WithMany(r => r.Assignments)
                .HasForeignKey(ra => ra.RobotId);

            modelBuilder.Entity<RobotAssignment>()
                .HasOne(pd => pd.Order)
                .WithOne(o => o.RobotAssignments)
                .HasForeignKey<RobotAssignment>(pd => pd.OrderId);

            modelBuilder.Entity<RobotSpecification>()
                .HasOne(rs => rs.Robot)
                .WithOne(r => r.Specification)
                .HasForeignKey<RobotSpecification>(rs => rs.RobotId);

            modelBuilder.Entity<RobotTelemetry>()
                .HasOne(rt => rt.Robot)
                .WithOne(r => r.Telemetry)
                .HasForeignKey<RobotTelemetry>(rt => rt.RobotId);

            modelBuilder.Entity<Edge>()
                .HasOne(e => e.FromNode)
                .WithMany(n => n.OutgoingEdges)
                .HasForeignKey(e => e.FromNodeId);

            modelBuilder.Entity<Edge>()
                .HasOne(e => e.ToNode)
                .WithMany(n => n.IncomingEdges)
                .HasForeignKey(e => e.ToNodeId);

            modelBuilder.Entity<RouteNode>()
                .HasOne(rn => rn.Node)
                .WithMany(n => n.RouteNodes)
                .HasForeignKey(rn => rn.NodeId);

            modelBuilder.Entity<Route>()
                .HasMany(r => r.RouteNodes)
                .WithOne(rn => rn.Route)
                .HasForeignKey(rn => rn.RouteId);

            modelBuilder.Entity<Order>()
                .HasMany(e => e.Routes)
                .WithOne(r => r.Order)
                .HasForeignKey(e => e.OrderId);
        }
    }
}