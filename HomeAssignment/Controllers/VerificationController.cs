using HomeAssignment.Filters;
using HomeAssignment.Models;
using HomeAssignment.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomeAssignment.Controllers
{
    public class VerificationController : Controller
    {
        private readonly string _adminEmail;
        private readonly IItemsRepository _db;

        public VerificationController(
            IConfiguration config,
            [FromKeyedServices("db")] IItemsRepository dbRepo)
        {
            _adminEmail = config["SiteAdminEmail"];
            _db = dbRepo;
        }

        public async Task<IActionResult> Index()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null) return RedirectToAction("Login", "Auth");

            var items = await _db.GetAsync();

            if (email == _adminEmail)
            {
                // Show pending restaurants
                var pendingRestaurants = items
                    .OfType<Restaurant>()
                    .Where(r => r.Status == ItemStatus.Pending)
                    .ToList();

                return View("AdminVerify", pendingRestaurants);
            }
            else
            {
                // Show owned restaurants
                var ownedRestaurants = items
                    .OfType<Restaurant>()
                    .Where(r => r.OwnerEmailAddress == email)
                    .ToList();

                return View("OwnerRestaurants", ownedRestaurants);
            }
        }

        public async Task<IActionResult> RestaurantMenuItems(int id)
        {
            var items = await _db.GetAsync();
            var pendingMenu = items
                .OfType<MenuItem>()
                .Where(m => m.RestaurantId == id && m.Status == ItemStatus.Pending)
                .ToList();

            return View("OwnerVerifyMenuItems", pendingMenu);
        }

        [HttpPost]
        [ServiceFilter(typeof(ApprovalAuthorizationFilter))]
        public async Task<IActionResult> Approve(int id)
        {
            await _db.Approve(id);
            return RedirectToAction("Index");
        }

    }
}
