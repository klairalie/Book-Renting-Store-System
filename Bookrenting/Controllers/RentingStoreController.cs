using Microsoft.AspNetCore.Mvc;
using BookRenting.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
    // DIGITAL validation
    if (string.Equals(model.BookType, "digital", StringComparison.OrdinalIgnoreCase))
    {
        if (model.ReturnDate == null || model.ReturnDate <= model.BorrowDate)
        {
            ModelState.AddModelError(nameof(model.ReturnDate), "Return date must be after borrow date for digital books.");
        }
    }

    // PHYSICAL book cannot be softcopy
    if (string.Equals(model.BookType, "physical", StringComparison.OrdinalIgnoreCase) &&
        string.Equals(model.BorrowType, "softcopy", StringComparison.OrdinalIgnoreCase))
    {
        ModelState.AddModelError(nameof(model.BorrowType), "Physical books is not allowed for softcopy.");
    }

    if (!ModelState.IsValid)
    {
        return BadRequest(new
        {
            success = false,
            message = "Please check your form inputs.",
            errors = ModelState.SelectMany(x => x.Value?.Errors.Select(e => e.ErrorMessage)
                      ?? Enumerable.Empty<string>()).ToArray()
        });
    }


    try
    {
        string? receiptPath = null;

        // GCASH RECEIPT UPLOAD
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

        // STOCK CHECK FOR PHYSICAL
        if (string.Equals(model.BookType, "physical", StringComparison.OrdinalIgnoreCase))
        {
            var book = await _context.Books
                .Where(b => b.Title == model.BookTitle)
                .FirstOrDefaultAsync();

            if (book == null)
            {
                return BadRequest(new { success = false, message = "Book not found in the catalog." });
            }

            if (book.Stocks <= 0)
            {
                return BadRequest(new { success = false, message = "This book is out of stock." });
            }

            book.Stocks -= 1;

            if (book.Stocks == 0)
            {
                book.Status = "Unavailable";
            }
        }

        // --------------------------------------------------------------
        // BACKEND LATE FEE CALCULATION (NOT ADDED TO PaymentTotal)
        // --------------------------------------------------------------

        decimal lateFee = 0;

        if (model.ReturnDate.HasValue)
        {
            DateTime today = DateTime.Now.Date;

            if (today > model.ReturnDate.Value)
            {
                int lateDays = (today - model.ReturnDate.Value).Days;
                lateFee = lateDays * 20;   // 20 PHP per day late
            }
        }

        // --------------------------------------------------------------
        // CREATE RENTEDBOOK ENTRY (LATEFEE SAVED BUT NOT ADDED TO TOTAL)
        // --------------------------------------------------------------

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

            PaymentTotal = model.PaymentTotal, // ORIGINAL PAYMENT TOTAL ONLY (NO LATE FEE)

            LateFee = lateFee, // <-- stored separately

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
        return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
    }
}

        [HttpGet]
        public IActionResult ThankYou()
        {
            return View();
        }

        public IActionResult MyBooks()
{
    var email = User?.Identity?.Name; // Or however you track user email
    var approvedBooks = _context.RentedBooks
                                .Where(b => b.Email == email && b.Status == "Approved")
                                .OrderByDescending(b => b.BorrowDate)
                                .ToList();
    return View("MyBooks", approvedBooks);
}

[HttpGet]
public IActionResult ReadNow(int rentId)
{
    var email = User?.Identity?.Name;

    var rented = _context.RentedBooks
        .FirstOrDefault(r => r.RentId == rentId && r.Email == email);

    if (rented == null)
        return NotFound("Rental not found.");

    var book = _context.Books
        .FirstOrDefault(b => b.Title == rented.BookTitle);

    if (book == null)
        return NotFound("Book not found.");

var model = new RentedBook
{
    BookTitle = rented.BookTitle,
    Author = rented.Author,
    FilePath = book.FilePath ?? "" // empty string if null
};

    return View("ReadNow", model);
}

    }
}
