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

        // -------------------------------------------------------------
        // MAIN VERIFICATION PAGE
        // Admin -> sees ALL pending restaurants
        // Owner -> sees OWN restaurants (not approved)
        // -------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null)
                return RedirectToAction("Login", "Auth");

            var items = await _db.GetAsync();

            // ---------------------------------------------------------
            // ADMIN VIEW
            // ---------------------------------------------------------
            if (email.Equals(_adminEmail, StringComparison.OrdinalIgnoreCase))
            {
                var pendingRestaurants = items
                    .OfType<Restaurant>()
                    .Where(r => r.Status == ItemStatus.Pending)
                    .ToList();

                return View("AdminVerify", pendingRestaurants);
            }

            // ---------------------------------------------------------
            // OWNER VIEW
            // ---------------------------------------------------------
            var ownedRestaurants = items
                .OfType<Restaurant>()
                .Where(r => r.OwnerEmailAddress.Equals(email, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return View("OwnerRestaurants", ownedRestaurants);
        }

        // -------------------------------------------------------------
        // OWNER VIEW → Click Restaurant → See Pending MenuItems
        // -------------------------------------------------------------
        public async Task<IActionResult> RestaurantMenuItems(int id)
        {
            var items = await _db.GetAsync();

            var pendingMenuItems = items
                .OfType<MenuItem>()
                .Where(m => m.RestaurantId == id && m.Status == ItemStatus.Pending)
                .ToList();

            return View("OwnerVerifyMenuItems", pendingMenuItems);
        }

        // -------------------------------------------------------------
        // APPROVE
        // Uses ActionFilter to confirm user is in GetValidators()
        // -------------------------------------------------------------
        [HttpPost]
        [ServiceFilter(typeof(ApprovalAuthorizationFilter))]
        public async Task<IActionResult> Approve(List<string> ids)
        {
            if (ids == null || ids.Count == 0)
                return BadRequest("No items selected.");

            foreach (var id in ids)
                await _db.Approve(id);

            return RedirectToAction("Index");
        }
    }
}
