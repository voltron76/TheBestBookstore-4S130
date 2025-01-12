using Microsoft.AspNetCore.Mvc;
using System.Runtime.Intrinsics.X86;
using TheBestBookstore.Data;
using TheBestBookstore.Models;

namespace TheBestBookstore.Controllers
{
    public class BooksController : Controller
    {
        private ApplicationDbContext db;

        public BooksController(ApplicationDbContext db)
        {
            this.db = db;   
        }

        public IActionResult Index()
        {
            List<Book> catalog = db.Books.ToList();

            return View(catalog);
        }
    }
}
