using HomeAssignment.Models;
using HomeAssignment.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomeAssignment.Controllers
{
    public class ItemsController : Controller
    {
        private readonly IItemsRepository _db;

        public ItemsController([FromKeyedServices("db")] IItemsRepository repo)
        {
            _db = repo;
        }

        // Shows approved restaurants
        public async Task<IActionResult> Catalog()
        {
            var items = await _db.GetAsync();
            var restaurants = items
                .OfType<Restaurant>()
                .Where(r => r.Status == ItemStatus.Approved)
                .ToList();

            return View(restaurants);
        }

        // Shows approved menu items belonging to a restaurant
        public async Task<IActionResult> Restaurant(int id)
        {
            var items = await _db.GetAsync();

            var restaurant = items
                .OfType<Restaurant>()
                .FirstOrDefault(r => r.Id == id);

            var menuItems = items
                .OfType<MenuItem>()
                .Where(m => m.RestaurantId == id && m.Status == ItemStatus.Approved)
                .ToList();

            ViewBag.Restaurant = restaurant;
            return View(menuItems);
        }
    }
}
