using Microsoft.AspNetCore.Mvc;

namespace Minitwit.Controllers
{
    public class MinitwitController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
