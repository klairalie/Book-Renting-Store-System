using Microsoft.AspNetCore.Mvc;
using BookRenting.Models;

namespace BookRenting.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult AdminDashboard()
        {
            int totalBooks = _context.Books.Count();
            ViewBag.TotalBooks = totalBooks;

            int totalUsers = _context.RegisterUsers.Count();
            ViewBag.TotalUsers = totalUsers;

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
