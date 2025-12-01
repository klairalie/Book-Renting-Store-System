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
            int totalBooks = _context.Books.Sum(b => b.Stocks);
            ViewBag.TotalBooks = totalBooks;

            int totalUsers = _context.RegisterUsers.Count();
            ViewBag.TotalUsers = totalUsers;

            int totalRents = _context.RentedBooks.Count();
            ViewBag.TotalRents = _context.RentedBooks.Count(r => r.Status == "Pending");

            var today = DateTime.Today;
            ViewBag.DailyRentCount = _context.RentedBooks
                .Count(r => r.BorrowDate.Date == today);

            var currentMonth = today.Month;
            var currentYear = today.Year;

            // --- Aggregate monthly stats from RentedBooks ---
            var monthlyRents = _context.RentedBooks
                .Where(r => r.BorrowDate.Month == currentMonth && r.BorrowDate.Year == currentYear)
                .ToList();

            int rentedCount = monthlyRents.Count;
            int returnedCount = monthlyRents.Count(r => r.Status == "Returned");
            int lateCount = monthlyRents.Count(r => r.Status == "Late");
            int lostCount = monthlyRents.Count(r => r.Status == "Lost");
            decimal totalSales = monthlyRents.Sum(r => r.PaymentTotal);

            // --- Store/update in Reports table ---
            var report = _context.Reports.FirstOrDefault(r => r.Date.Month == currentMonth && r.Date.Year == currentYear);
            if (report == null)
            {
                report = new Report
                {
                    Date = today,
                    BooksRented = rentedCount,
                    BooksReturned = returnedCount,
                    BooksLate = lateCount,
                    BooksLost = lostCount,
                    TotalSales = totalSales
                };
                _context.Reports.Add(report);
            }
            else
            {
                report.BooksRented = rentedCount;
                report.BooksReturned = returnedCount;
                report.BooksLate = lateCount;
                report.BooksLost = lostCount;
                report.TotalSales = totalSales;
            }
            _context.SaveChanges();

            // --- Chart data from Reports ---
            var chartData = _context.Reports
                .Where(r => r.Date.Month == currentMonth && r.Date.Year == currentYear)
                .OrderBy(r => r.Date.Day)
                .Select(r => new
                {
                    Day = r.Date.Day,
                    Rented = r.BooksRented,
                    Returned = r.BooksReturned,
                    Late = r.BooksLate,
                    Lost = r.BooksLost,
                    Total = r.TotalSales
                })
                .ToList();

            ViewBag.ChartData = chartData;

            // --- Insights/Recommendations ---
            string insights = "";
            if (lateCount > 5) insights += "High number of late returns. Consider sending reminders or applying late fees.\n";
            if (lostCount > 0) insights += "Some books are lost. Review deposit or replacement policies.\n";
            if (returnedCount == 0 && rentedCount > 0) insights += "No books returned yet this month. Check for delays.\n";
            if (insights == "") insights = "No major issues detected this month.";
            ViewBag.Insights = insights;

            ViewBag.MonthlySales = totalSales;
            ViewBag.ReturnedCount = returnedCount;
            ViewBag.LateCount = lateCount;
            ViewBag.LostCount = lostCount;

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
