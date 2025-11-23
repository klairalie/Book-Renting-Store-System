using Microsoft.AspNetCore.Mvc;

namespace BookRenting.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult AdminDashboard()
        {
            return View();

        }

        public IActionResult Test()
        {
            return Content("Admin Controller Works!");
        }

        public IActionResult Logout()
        {
            return View("Login", "Account");
        }
    }
}
