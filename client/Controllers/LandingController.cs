using Microsoft.AspNetCore.Mvc;

namespace RegClient.Controllers
{
    public class LandingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
