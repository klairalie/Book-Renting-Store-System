using Microsoft.AspNetCore.Mvc;
using BookRenting.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;

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
                FullName = user != null ? $"{user.FirstName} {user.LastName}" : string.Empty,
                Address = user?.Address ?? string.Empty,
                ContactNumber = user?.ContactNumber ?? string.Empty,
                Email = user?.Email ?? string.Empty
            };

            return View(rent);
        }
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult RentBookSubmit(RentedBook model, IFormFile? ReceiptFile, string? amountPaid)
{
    if (!ModelState.IsValid)
    {
        return View("RentBook", model);
    }

    string? receiptPath = null;

    // Receipt upload handling for GCash
    if (model.PaymentMode == "gcash" && ReceiptFile != null)
    {
        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/receipts");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        string fileName = $"gcash_{Guid.NewGuid()}{Path.GetExtension(ReceiptFile.FileName)}";
        string filePath = Path.Combine(uploadsFolder, fileName);

        using (var fs = new FileStream(filePath, FileMode.Create))
        {
            ReceiptFile.CopyTo(fs);
        }

        receiptPath = $"/uploads/receipts/{fileName}";
    }

    // Create new DB record
    var rent = new RentedBook
    {
        FullName = model.FullName,
        Address = model.Address,
        Email = model.Email,
        ContactNumber = model.ContactNumber,

        BookTitle = model.BookTitle,
        Author = model.Author,
        BookType = model.BookType,

        BorrowDate = model.BorrowDate,
        ReturnDate = model.ReturnDate,

        BookPrice = model.BookPrice,
        BorrowType = model.BorrowType,
        Deposit = model.Deposit,
        ShippingFee = model.ShippingFee,

        PaymentMode = model.PaymentMode,
        PaymentTotal = model.PaymentTotal,

        ReferenceNumber = model.ReferenceNumber ?? Guid.NewGuid().ToString().Substring(0, 10).ToUpper(),
        ReceiptPath = receiptPath,
        Status = "Pending"
    };

    _context.RentedBooks.Add(rent);
    _context.SaveChanges();

    return RedirectToAction("BrowseCatalog", "Books");
}

      
        [HttpGet]
        public IActionResult ThankYou()
        {
            return View();
        }
    }
}
