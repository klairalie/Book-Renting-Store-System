using Microsoft.AspNetCore.Mvc;
using BookRenting.Models;
using Microsoft.EntityFrameworkCore;

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

    var today = DateTime.Today;

    ViewBag.DailyRentCount = _context.RentedBooks
        .Count(r => r.BorrowDate.Date == today);

    var currentMonth = today.Month;
    var currentYear = today.Year;

    ViewBag.MonthlySales = _context.RentedBooks
        .Where(r => r.BorrowDate.Month == currentMonth && r.BorrowDate.Year == currentYear)
        .Sum(r => (decimal?)r.PaymentTotal) ?? 0;

    // --- Data for Chart.js ---
    var chartData = _context.RentedBooks
        .Where(r => r.BorrowDate.Month == currentMonth && r.BorrowDate.Year == currentYear)
        .GroupBy(r => r.BorrowDate.Day)
        .Select(g => new
        {
            Day = g.Key,
            Count = g.Count(),
            Total = g.Sum(x => x.PaymentTotal)
        })
        .OrderBy(g => g.Day)
        .ToList();

    ViewBag.ChartData = chartData;

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

    // GET: /Admin/ApprovedRents
// GET: /Admin/ApprovedRents
public async Task<IActionResult> ApprovedRents()
{
    // Fetch all rented books, regardless of status
    var allRents = await _context.RentedBooks
    .OrderBy(rb => rb.Status == "Pending" ? 0 : 1)
    .ThenBy(rb => rb.BorrowDate)
    .ToListAsync();


    return View("ApproveRents", allRents);
}

    [HttpPost]
public async Task<IActionResult> MarkRentAjax([FromBody] MarkRentDto dto)
{
    var rent = await _context.RentedBooks.FindAsync(dto.RentId);
    if (rent == null) return NotFound();

    rent.Status = dto.Status;
    await _context.SaveChangesAsync();

    return Ok();
}

public class MarkRentDto
{
    public int RentId { get; set; }
    public string Status { get; set; } = string.Empty;
}
        
}

}
