using HomeAssignment.Models;
using HomeAssignment.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomeAssignment.Controllers
{
    public class ItemsController : Controller
    {
        private readonly IItemsRepository _db;

        public ItemsController([FromKeyedServices("db")] IItemsRepository dbRepo)
        {
            _db = dbRepo;
        }

      
        public async Task<IActionResult> Catalog(string mode = "restaurants", int? restaurantId = null)
        {
            var allItems = await _db.GetAsync();
            List<IItemValidating> model;

            if (string.Equals(mode, "menu", StringComparison.OrdinalIgnoreCase) && restaurantId.HasValue)
            {
             
                model = allItems
                    .OfType<MenuItem>()
                    .Where(m => m.RestaurantId == restaurantId.Value
                                && m.Status == ItemStatus.Approved)
                    .Cast<IItemValidating>()
                    .ToList();

                ViewBag.Mode = "menu";
                ViewBag.RestaurantId = restaurantId;
            }
            else
            {
                
                model = allItems
                    .OfType<Restaurant>()
                    .Where(r => r.Status == ItemStatus.Approved)
                    .Cast<IItemValidating>()
                    .ToList();

                ViewBag.Mode = "restaurants";
                ViewBag.RestaurantId = null;
            }

            return View(model);  
        }
    }
}
