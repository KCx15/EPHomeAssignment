using HomeAssignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeAssignment.Data
{
    public class CatalogController : Controller
    {
        private readonly AppDbContext _db;

        public CatalogController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var restaurants = await _db.Restaurants.ToListAsync();
            var menuItems = await _db.MenuItems.Include(m => m.Restaurant).ToListAsync();

            var combined = new List<IItemValidating>();
            combined.AddRange(restaurants);
            combined.AddRange(menuItems);

            return View("Catalog", combined);
        }
    }

}
