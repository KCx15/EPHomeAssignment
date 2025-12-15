using HomeAssignment.Data;
using HomeAssignment.Models;
using Microsoft.AspNetCore.Mvc;

namespace HomeAssignment.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _ctx;

        public AuthController(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            email = email.Trim();

            var exists = _ctx.Users.Any(u => u.Email == email);
            if (exists)
            {
                ViewBag.Error = "An account with this email already exists.";
                return View();
            }

            _ctx.Users.Add(new AppUser { Email = email, Password = password });
            _ctx.SaveChanges();

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            email = email.Trim();

            var user = _ctx.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            HttpContext.Session.SetString("UserEmail", user.Email);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
