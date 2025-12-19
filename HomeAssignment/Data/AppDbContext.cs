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

            
            modelBuilder.Entity<Restaurant>()
                .HasIndex(r => r.ExternalId)
                .IsUnique();

           
            modelBuilder.Entity<MenuItem>()
                .HasIndex(m => m.ExternalId)
                .IsUnique();

            
            modelBuilder.Entity<MenuItem>()
                .HasOne(m => m.Restaurant)
                .WithMany(r => r.MenuItems)
                .HasForeignKey(m => m.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

           
        }
    }
}
