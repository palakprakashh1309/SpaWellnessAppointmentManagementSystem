
using Microsoft.AspNetCore.Mvc;

namespace SerenitySpa.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Admin() => View();
        public IActionResult Staff() => View();
        public IActionResult Customer() => View();
    }
}

