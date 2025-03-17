using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBestBookstore.Data;
using TheBestBookstore.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using TheBestBookstore.Extensions;

namespace TheBestBookstore.Controllers
{
    [Route("Books")]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public BooksController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet("")]
        public IActionResult Index(string searchTerm, int? categoryId)
        {
            var query = _context.Books.Include(b => b.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(b => 
                    b.Title.ToLower().Contains(searchTerm) || 
                    b.Author.ToLower().Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", categoryId);

            return View(query.ToList());
        }
        
        [HttpGet]
        [Route("Details/{id}")]
        public IActionResult Details(int id)
        {
            var book = _context.Books.Include(b => b.Category)
                .FirstOrDefault(b => b.Id == id);
                
            if (book == null)
            {
                return NotFound();
            }
            
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View(book);
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View(new BookViewModel { Published = DateTime.Now });
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check for duplicate ISBN to prevent shadow books
                    if (!string.IsNullOrEmpty(viewModel.ISBN))
                    {
                        bool duplicateISBN = await _context.Books.AnyAsync(b => b.ISBN == viewModel.ISBN);
                        if (duplicateISBN)
                        {
                            ModelState.AddModelError("ISBN", "A book with this ISBN already exists");
                            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", viewModel.CategoryId);
                            return View(viewModel);
                        }
                    }

                    // Create book entity from view model
                    var book = new Book
                    {
                        Title = viewModel.Title,
                        Author = viewModel.Author,
                        Description = viewModel.Description,
                        ISBN = viewModel.ISBN ?? "",
                        CategoryId = viewModel.CategoryId,
                        Published = viewModel.Published,
                        Pages = viewModel.Pages,
                        Price = viewModel.Price,
                        IsBestSeller = viewModel.IsBestSeller,
                        DateAdded = DateTime.Now
                    };

                    // Handle image - prioritize file upload over URL
                    if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "books");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ImageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        // Ensure directory exists
                        Directory.CreateDirectory(uploadsFolder);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await viewModel.ImageFile.CopyToAsync(fileStream);
                        }
                        
                        book.ImageUrl = "/images/books/" + uniqueFileName;
                    }
                    else if (!string.IsNullOrEmpty(viewModel.ImageUrl))
                    {
                        book.ImageUrl = viewModel.ImageUrl;
                    }

                    _context.Books.Add(book);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Book '{book.Title}' was successfully added.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating book: " + ex.Message);
            }
            
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", viewModel.CategoryId);
            return View(viewModel);
        }

        [HttpGet]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        [HttpPost]
        [Route("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)  // Remove [FromBody]
        {
            if (id != book.Id)
            {
                return Json(new { success = false, message = "Invalid book ID" });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = string.Join("; ", ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)) 
                });
            }

            try
            {
                var existingBook = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                if (existingBook == null)
                {
                    return Json(new { success = false, message = "Book not found" });
                }

                // Preserve values that shouldn't be modified
                book.DateAdded = existingBook.DateAdded;
                book.ImageUrl = existingBook.ImageUrl;
                book.ISBN = existingBook.ISBN; // Preserve ISBN as well

                _context.Update(book);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [Route("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            try
            {
                if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                    var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "images", book.ImageUrl);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }

        [HttpGet("SearchBook")]
        public IActionResult SearchBook(string isbn)
        {
            var results = _context.Books
                .Where(b => b.ISBN.Contains(isbn))
                .ToList();
            // Return a partial view with the results
            return PartialView("_SearchResults", results);
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
