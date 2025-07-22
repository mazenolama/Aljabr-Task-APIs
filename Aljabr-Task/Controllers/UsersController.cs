using Microsoft.AspNetCore.Mvc;

namespace Aljabr_Task.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
