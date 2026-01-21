using Microsoft.AspNetCore.Mvc;

namespace SerenitySpa.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult Services() => View();

        // Add this for /Home/About
        public IActionResult About()
        {
            return View();
        }

        // Add this for /Home/Contact
        public IActionResult Contact()
        {
            return View();
        }

    }
}

