
using Microsoft.AspNetCore.Mvc;

namespace SerenitySpa.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult Services() => View();
    }
}

