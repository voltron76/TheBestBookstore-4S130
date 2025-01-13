using Microsoft.AspNetCore.Mvc;
using TheBestBookstore.Data;
using TheBestBookstore.Models;

namespace TheBestBookstore.Controllers
{
    public class CategoriesController : Controller
    {
        private ApplicationDbContext db;

        public CategoriesController(ApplicationDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Category> categories = db.Categories.ToList();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            db.Categories.Add(category);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
