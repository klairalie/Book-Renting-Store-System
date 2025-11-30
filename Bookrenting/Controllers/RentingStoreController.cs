using Microsoft.AspNetCore.Mvc;
using BookRenting.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace BookRenting.Controllers
{
    public class RentingStoreController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public RentingStoreController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RentBook(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (book == null) return NotFound();

            var userEmail = User?.Identity?.Name;
            var user = !string.IsNullOrEmpty(userEmail)
                ? _context.RegisterUsers.FirstOrDefault(u => u.Email == userEmail)
                : null;

                     var rent = new RentedBook
{
    BookTitle = book.Title,
    Status = book.Status,
    Author = book.Author,
    BookPrice = book.Price,
    BookType = book.Status?.Split(' ').Last() ?? "", // FIXED
    FullName = user != null ? $"{user.FirstName} {user.LastName}" : string.Empty,
    Address = user?.Address ?? string.Empty,
    ContactNumber = user?.ContactNumber ?? string.Empty,
    Email = user?.Email ?? string.Empty,
    BorrowDate = DateTime.Today
};

            return View(rent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RentBookSubmit(RentedBook model, IFormFile? ReceiptFile)
        {
            // Server-side: ensure required fields for digital
            if (string.Equals(model.BookType, "digital", StringComparison.OrdinalIgnoreCase))
            {
                // For digital, make sure ReturnDate is present and valid
                if (model.ReturnDate == null || model.ReturnDate <= model.BorrowDate)
                {
                    ModelState.AddModelError(nameof(model.ReturnDate), "Return date must be after borrow date for digital books.");
                }
            }

            if (!ModelState.IsValid)
            {
                // Return BadRequest so that front-end can detect and show error
                return BadRequest(new { success = false, message = "Please check your form inputs.", errors = ModelState.SelectMany(x => x.Value?.Errors.Select(e => e.ErrorMessage) ?? Enumerable.Empty<string>()).ToArray() });
            }

            try
            {
                string? receiptPath = null;

                // Upload receipt only for GCash
                if (string.Equals(model.PaymentMode, "gcash", StringComparison.OrdinalIgnoreCase) && ReceiptFile != null)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath ?? ".", "uploads", "receipts");

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string fileName = $"gcash_{Guid.NewGuid()}{Path.GetExtension(ReceiptFile.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ReceiptFile.CopyToAsync(fileStream);
                    }

                    receiptPath = $"/uploads/receipts/{fileName}";
                }

                // Ensure numeric properties have sensible defaults (avoid nulls causing db issues)
                model.Deposit = model.Deposit;
                model.ShippingFee = model.ShippingFee;
                model.PaymentTotal = model.PaymentTotal;
                model.AmountPaid = model.AmountPaid;

                var rented = new RentedBook
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    ContactNumber = model.ContactNumber,
                    Address = model.Address,
                    BookTitle = model.BookTitle,
                    Author = model.Author,
                    BookType = model.BookType,
                    BookPrice = model.BookPrice,
                    BorrowDate = model.BorrowDate,
                    ReturnDate = model.ReturnDate,
                    BorrowType = model.BorrowType,
                    Deposit = model.Deposit,
                    ShippingFee = model.ShippingFee,
                    PaymentTotal = model.PaymentTotal,
                    PaymentMode = model.PaymentMode,
                    ReferenceNumber = model.ReferenceNumber,
                    AmountPaid = model.AmountPaid,
                    ReceiptPath = receiptPath,
                    Status = "Pending",
                };

                _context.RentedBooks.Add(rented);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Rent request submitted successfully!" });
            }
            catch (Exception)
            {
                // log ex as needed
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }

        [HttpGet]
        public IActionResult ThankYou()
        {
            return View();
        }
    }
}
