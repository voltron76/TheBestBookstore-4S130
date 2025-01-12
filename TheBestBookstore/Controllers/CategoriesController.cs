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

        public IActionResult Index()
        {
            List<Category> categories = db.Categories.ToList();
            return View(categories);
        }
    }
}
