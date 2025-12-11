using HomeAssignment.Data;
using HomeAssignment.Models;
using Microsoft.AspNetCore.Mvc;

namespace HomeAssignment.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly IConfiguration _config;


        public AuthController(AppDbContext ctx, IConfiguration config)
        {
            _ctx = ctx;
            _config = config;
        }

        public IActionResult Register() => View();
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Register(string email, string password)
        {
            _ctx.Users.Add(new AppUser { Email = email, Password = password });
            _ctx.SaveChanges();

            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _ctx.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user == null)
                return View();

            HttpContext.Session.SetString("UserEmail", email);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
