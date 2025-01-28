using Microsoft.AspNetCore.Mvc;

namespace Bitirme.Controllers
{
    public class HelloController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
