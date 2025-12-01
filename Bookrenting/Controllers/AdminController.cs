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
    // --- Summary Cards ---
    ViewBag.TotalBooks = _context.Books.Sum(b => b.Stocks);
    ViewBag.TotalUsers = _context.RegisterUsers.Count();
    ViewBag.TotalRents = _context.RentedBooks.Count(r => r.Status == "Pending");

    var today = DateTime.Today;
    ViewBag.DailyRentCount = _context.RentedBooks
        .Count(r => r.BorrowDate.Date == today && r.Status == "Approved");

    var currentMonth = today.Month;
    var currentYear = today.Year;

    // --------------------------------------------
    // MONTHLY COUNTS (Corrected)
    // --------------------------------------------

    // Approved = BorrowDate month
    int approvedCount = _context.RentedBooks
        .Count(r => r.Status == "Approved" &&
                    r.BorrowDate.Month == currentMonth &&
                    r.BorrowDate.Year == currentYear);

    // Returned = ReturnDate month
    int returnedCount = _context.RentedBooks
        .Count(r => r.Status == "Returned" &&
                    r.ReturnDate.HasValue &&
                    r.ReturnDate.Value.Month == currentMonth &&
                    r.ReturnDate.Value.Year == currentYear);

    // Late Return = ReturnDate month
    int lateCount = _context.RentedBooks
        .Count(r => r.Status == "Late Return" &&
                    r.ReturnDate.HasValue &&
                    r.ReturnDate.Value.Month == currentMonth &&
                    r.ReturnDate.Value.Year == currentYear);

    // Lost = ReturnDate month OR LostDate if you have one
    int lostCount = _context.RentedBooks
        .Count(r => r.Status == "Lost" &&
                    r.ReturnDate.HasValue &&
                    r.ReturnDate.Value.Month == currentMonth &&
                    r.ReturnDate.Value.Year == currentYear);

    // SALES = any month where payment happened (Approved OR Returned)
    decimal totalSales = _context.RentedBooks
        .Where(r =>
            (r.Status == "Approved" &&
             r.BorrowDate.Month == currentMonth &&
             r.BorrowDate.Year == currentYear)
            ||
            ((r.Status == "Returned" || r.Status == "Late Return") &&
             r.ReturnDate.HasValue &&
             r.ReturnDate.Value.Month == currentMonth &&
             r.ReturnDate.Value.Year == currentYear)
        )
        .Sum(r => r.AmountPaid);

    // --------------------------------------------
    // UPDATE REPORT TABLE
    // --------------------------------------------
    var report = _context.Reports
        .FirstOrDefault(r => r.Date.Month == currentMonth && r.Date.Year == currentYear);

    if (report == null)
    {
        _context.Reports.Add(new Report
        {
            Date = today,
            BooksApproved = approvedCount,
            BooksReturned = returnedCount,
            BooksLate = lateCount,
            BooksLost = lostCount,
            TotalSales = totalSales
        });
    }
    else
    {
        report.BooksApproved = approvedCount;
        report.BooksReturned = returnedCount;
        report.BooksLate = lateCount;
        report.BooksLost = lostCount;
        report.TotalSales = totalSales;
    }

    _context.SaveChanges();

    // --------------------------------------------
    // CHART DATA
    // --------------------------------------------
    ViewBag.ChartData = _context.Reports
        .Where(r => r.Date.Month == currentMonth && r.Date.Year == currentYear)
        .OrderBy(r => r.Date.Day)
        .Select(r => new
        {
            Day = r.Date.Day,
            Approved = r.BooksApproved,
            Returned = r.BooksReturned,
            Late = r.BooksLate,
            Lost = r.BooksLost,
            Total = r.TotalSales
        })
        .ToList();

    // --------------------------------------------
    // INSIGHTS
    // --------------------------------------------
    string insights = "";
    if (lateCount > 5)
        insights += "High number of late returns. Consider sending reminders or applying late fees.\n";
    if (lostCount > 0)
        insights += "Some books are lost. Review deposit or replacement policies.\n";
    if (returnedCount == 0 && approvedCount > 0)
        insights += "No books returned yet this month. Check for delays.\n";
    if (string.IsNullOrEmpty(insights))
        insights = "No major issues detected this month.";

    ViewBag.Insights = insights;
    ViewBag.MonthlySales = totalSales;
    ViewBag.ReturnedCount = returnedCount;
    ViewBag.LateCount = lateCount;
    ViewBag.LostCount = lostCount;

    return View();
}
        public IActionResult Test() => Content("Admin Controller Works!");
        public IActionResult Logout() => View("Login", "Account");

      public async Task<IActionResult> ApprovedRents()
{
    var allRents = await _context.RentedBooks
        .OrderByDescending(r => r.Status == "Pending") // Pending first
        .ThenBy(r => r.BorrowDate)                   // Then by borrow date
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
