using HomeAssignment.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;


namespace HomeAssignment.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
    }
}
