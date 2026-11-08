using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MinibusTakip.Models;

namespace MinibusTakip.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Stop> Stops { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<VehicleStatus> VehicleStatuses { get; set; }
        public DbSet<VehicleLocation> VehicleLocations { get; set; }
        public DbSet<DelayUpdate> DelayUpdates { get; set; }
        public DbSet<DriverMessage> DriverMessages { get; set; }
        public DbSet<StopSubscription> StopSubscriptions { get; set; }
        public DbSet<UserPushSubscription> UserPushSubscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Bir minibüsün yalnızca bir anlık durumu olur
            builder.Entity<VehicleStatus>()
                   .HasIndex(v => v.VehicleId)
                   .IsUnique();

            // Stop.Sira sıralı arama için index
            builder.Entity<Stop>()
                   .HasIndex(s => s.Sira);
        }
    }
}