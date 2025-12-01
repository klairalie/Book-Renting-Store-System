using Microsoft.AspNetCore.Mvc;
using BookRenting.Models;
using Microsoft.EntityFrameworkCore;

namespace BookRenting.Controllers
{
    public class ReturnBookController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReturnBookController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ReturnBook?bookId=123
        [HttpGet]
        public async Task<IActionResult> Index(int bookId)
        {
            // Check if a return record already exists
            var returnBook = await _context.ReturnBooks.FindAsync(bookId);

            if (returnBook == null)
            {
                returnBook = new ReturnBook
                {
                    Id = bookId,
                    BorrowDate = DateTime.Now,
                    LateFee = 0,
                    PaymentTotal = 0,
                    AmountPaid = 0,
                    ReturnType = "Walk-in",
                    PaymentMode = "Cash",
                    Status = "Pending",
                    PaymentStatus = "Pending"
                };
            }

            return View("ReturnBook", returnBook);
        }

        // POST: ReturnBook
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            int Id,
            string BookTitle,
            string BookType,
            DateTime BorrowDate,
            decimal LateFee,
            decimal PaymentTotal,
            decimal AmountPaid,
            string ReturnType,
            string PaymentMode,
            string ReferenceNumber)
        {
            if (string.IsNullOrEmpty(BookTitle) || string.IsNullOrEmpty(BookType))
                return BadRequest("Invalid form data.");

            var returnBook = new ReturnBook
            {
                Id = Id,
                BookTitle = BookTitle,
                BookType = BookType,
                BorrowDate = BorrowDate,
                ReturnDate = DateTime.Now,
                LateFee = LateFee,
                PaymentTotal = PaymentTotal,
                AmountPaid = AmountPaid,
                ReturnType = ReturnType,
                PaymentMode = PaymentMode,
                ReferenceNumber = ReferenceNumber,
                Status = "Returned",
                PaymentStatus = AmountPaid >= PaymentTotal ? "Paid" : "Pending"
            };

            _context.ReturnBooks.Add(returnBook);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Book return submitted successfully!";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
