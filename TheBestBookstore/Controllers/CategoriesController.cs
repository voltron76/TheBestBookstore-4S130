using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBestBookstore.Models;
using TheBestBookstore.Data;
using System.Threading.Tasks;
using System.Linq;

namespace TheBestBookstore.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext db;

        public CategoriesController(ApplicationDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await db.Categories.ToListAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await db.Categories.FindAsync(id);
            if (category is null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id || !ModelState.IsValid)
            {
                return View(category);
            }

            try 
            {
                db.Categories.Update(category);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.Id))
                {
                    return NotFound();
                }
                ModelState.AddModelError("", "Category was modified by another user");
                return View(category);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await db.Categories.FindAsync(id);
            if (category is null)
            {
                return NotFound();
            }

            // Check if category has books
            var hasBooks = await db.Books.AnyAsync(b => b.CategoryId == id);
            if (hasBooks)
            {
                ViewData["ErrorMessage"] = "Cannot delete this category because it contains books. Remove or reassign all books first.";
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Important: Use ActionName("Delete") to match the form action
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await db.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Check if category has books
            var hasBooks = await db.Books.AnyAsync(b => b.CategoryId == id);
            if (hasBooks)
            {
                ModelState.AddModelError(string.Empty, "Cannot delete this category because it contains books. Remove or reassign all books first.");
                return View(category);
            }

            try 
            {
                db.Categories.Remove(category);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete category. It may have associated books.");
                return View(category);
            }
        }

        private bool CategoryExists(int id)
        {
            return db.Categories.Any(e => e.Id == id);
        }

        public async Task<IActionResult> SearchById(int id)
        {
            var category = await db.Categories.FindAsync(id);
            if (category is null)
            {
                return NotFound();
            }
            return View("Edit", category);
        }
    }
}
