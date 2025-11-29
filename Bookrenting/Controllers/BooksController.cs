using BookRenting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookRenting.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BooksController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.ToListAsync();
            return View(books);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

       [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Book book, IFormFile? bookFile, IFormFile? bookImage)
{
    if (ModelState.IsValid)
    {
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // Digital book file (PDF)
        if (bookFile != null && bookFile.Length > 0)
        {
            var ext = Path.GetExtension(bookFile.FileName).ToLower();
            if (ext != ".pdf")
            {
                ModelState.AddModelError("FilePath", "Digital book file must be a PDF.");
                return View(book);
            }

            var fileName = Guid.NewGuid() + ext;
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await bookFile.CopyToAsync(stream);
            }

            book.FilePath = "/uploads/" + fileName;
        }

        // Physical book image
        if (bookImage != null && bookImage.Length > 0)
        {
            var ext = Path.GetExtension(bookImage.FileName).ToLower();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".gif")
            {
                ModelState.AddModelError("ImagePath", "Book image must be JPG, JPEG, PNG, or GIF.");
                return View(book);
            }

            var fileName = Guid.NewGuid() + ext;
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await bookImage.CopyToAsync(stream);
            }

            book.ImagePath = "/uploads/" + fileName;
        }

        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    return View(book);
}

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

   [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Book book, IFormFile? bookFile, IFormFile? bookImage)
{
    if (id != book.Id)
        return NotFound();

    var existingBook = await _context.Books.FindAsync(id);
    if (existingBook == null) return NotFound();

    if (!ModelState.IsValid)
        return View(existingBook);

    // Update basic info
    existingBook.ReferenceNumber = book.ReferenceNumber;
    existingBook.Title = book.Title;
    existingBook.Author = book.Author;
    existingBook.Genre = book.Genre;
    existingBook.Status = book.Status;
    existingBook.Price = book.Price;

    if (!string.IsNullOrEmpty(book.Synopsis))
        existingBook.Synopsis = book.Synopsis;

    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
    if (!Directory.Exists(uploadsFolder))
        Directory.CreateDirectory(uploadsFolder);

    // Update digital file only if uploaded
    if (bookFile != null && bookFile.Length > 0)
    {
        var ext = Path.GetExtension(bookFile.FileName).ToLower();
        if (ext != ".pdf")
        {
            ModelState.AddModelError("bookFile", "Digital book file must be a PDF.");
            return View(existingBook);
        }

        var fileName = Guid.NewGuid() + ext;
        var filePath = Path.Combine(uploadsFolder, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await bookFile.CopyToAsync(stream);

        existingBook.FilePath = "/uploads/" + fileName;
    }

    // Update image only if uploaded
    if (bookImage != null && bookImage.Length > 0)
    {
        var ext = Path.GetExtension(bookImage.FileName).ToLower();
        if (!new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(ext))
        {
            ModelState.AddModelError("bookImage", "Book image must be JPG, JPEG, PNG, or GIF.");
            return View(existingBook);
        }

        var fileName = Guid.NewGuid() + ext;
        var filePath = Path.Combine(uploadsFolder, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await bookImage.CopyToAsync(stream);

        existingBook.ImagePath = "/uploads/" + fileName;
    }

    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Book updated successfully!";
    return RedirectToAction(nameof(Index));
}




        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

       // GET: Books/BrowseCatalog
// GET: Books/BrowseCatalog
public async Task<IActionResult> BrowseCatalog()
{
    var books = await _context.Books.ToListAsync();
    return View("~/Views/RentingStore/BrowseCatalog.cshtml", books);
}


    }
}
