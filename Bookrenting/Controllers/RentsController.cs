using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using BookRenting.Models;

public class RentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public RentsController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // Save Rent Request
    [HttpPost]
    public IActionResult RentBook(RentedBook rent)
    {
        if (rent.BorrowType.ToLower() == "ship")
        {
            rent.Deposit = 500;
            rent.ShippingFee = 100;
        }
        else if (rent.BorrowType.ToLower() == "walkin")
        {
            rent.Deposit = 150 + rent.BookPrice;
            rent.ShippingFee = 0;
        }

        rent.ReferenceNumber = Guid.NewGuid().ToString().Substring(0, 10).ToUpper();
        rent.Status = "Pending";

        if (rent.BorrowDate == default)
            rent.BorrowDate = DateTime.Now;

        _context.RentedBooks.Add(rent);
        _context.SaveChanges();

        return RedirectToAction("ThankYou");
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
            ReferenceNumber = model.ReferenceNumber ?? "",
            ReceiptPath = receiptPath,
            Status = "Pending"
        };

        _context.RentedBooks.Add(rent);
        _context.SaveChanges();

        return RedirectToAction("ThankYou");
    }

    public IActionResult ThankYou()
    {
        return View();
    }
}
