using Microsoft.AspNetCore.Mvc;

namespace BookRenting.Controllers
{
    public class RentingStoreController : Controller
    {
        // ---------------- GET Dashboard ----------------
        [HttpGet]
        public IActionResult Dashboard()
        {
            // You can pass a model or ViewData here if needed
            return View();
        }
    }
}
