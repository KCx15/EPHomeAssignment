using HomeAssignment.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeAssignment.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<AppUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Restaurant ExternalId must be unique (e.g. "R-1001")
            modelBuilder.Entity<Restaurant>()
                .HasIndex(r => r.ExternalId)
                .IsUnique();

            // MenuItem ExternalId must be unique (e.g. "M-2001")
            modelBuilder.Entity<MenuItem>()
                .HasIndex(m => m.ExternalId)
                .IsUnique();

            // Relationship: Restaurant 1 -> Many MenuItems
            modelBuilder.Entity<MenuItem>()
                .HasOne(m => m.Restaurant)
                .WithMany(r => r.MenuItems)
                .HasForeignKey(m => m.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // MenuItem.RestaurantExternalId is import-only (NotMapped),
            // EF ignores it automatically due to [NotMapped], so no config needed.
        }
    }
}
