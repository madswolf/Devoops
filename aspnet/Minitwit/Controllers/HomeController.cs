using Microsoft.AspNetCore.Mvc;

namespace Minitwit.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Minitwit");
        }
    }
}
