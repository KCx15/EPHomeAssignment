using Microsoft.AspNetCore.Mvc;

namespace HomeAssignment.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
