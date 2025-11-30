// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.Http;
// using System.IO;
// using BookRenting.Models;

// public class RentsController : Controller
// {
//     private readonly ApplicationDbContext _context;
//     private readonly IWebHostEnvironment _env;

//     public RentsController(ApplicationDbContext context, IWebHostEnvironment env)
//     {
//         _context = context;
//         _env = env;
//     }

//     // Save Rent Request
//     [HttpPost]
//     public IActionResult RentBook(RentedBook rent)
//     {
//         if (rent.BorrowType.ToLower() == "ship")
//         {
//             rent.Deposit = 500;
//             rent.ShippingFee = 100;
//         }
//         else if (rent.BorrowType.ToLower() == "walkin")
//         {
//             rent.Deposit = 150 + rent.BookPrice;
//             rent.ShippingFee = 0;
//         }

//         rent.ReferenceNumber = Guid.NewGuid().ToString().Substring(0, 10).ToUpper();
//         rent.Status = "Pending";

//         if (rent.BorrowDate == default)
//             rent.BorrowDate = DateTime.Now;

//         _context.RentedBooks.Add(rent);
//         _context.SaveChanges();

//         return RedirectToAction("ThankYou");
//     }

// }
