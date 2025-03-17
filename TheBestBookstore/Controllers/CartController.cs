using Microsoft.AspNetCore.Mvc;
using TheBestBookstore.Models;
using TheBestBookstore.Services;

namespace TheBestBookstore.Controllers
{
    [Route("[controller]")]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new CartViewModel
            {
                CartItems = _cartService.GetCartItems(),
                CartTotal = _cartService.GetTotal()
            };
            return View(model);
        }

        [HttpPost]
        [Route("add")]
        public IActionResult AddToCart([FromBody] CartAddRequest request)
        {
            try
            {
                if (request == null || request.BookId <= 0)
                {
                    return Json(new { success = false, message = "Invalid book" });
                }

                if (request.Quantity <= 0)
                {
                    return Json(new { success = false, message = "Invalid quantity" });
                }

                var book = _cartService.GetBook(request.BookId);
                if (book == null)
                {
                    return Json(new { success = false, message = "Book not found" });
                }

                _cartService.AddToCart(request.BookId, request.Quantity);
                return Json(new { success = true, message = "Book added to cart successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("update")]
        public IActionResult UpdateQuantity([FromBody] CartUpdateRequest request)
        {
            _cartService.UpdateQuantity(request.ItemId, request.Quantity);
            var newTotal = _cartService.GetTotal();
            return Json(new { success = true, total = newTotal });
        }

        [HttpPost]
        [Route("remove")]
        public IActionResult RemoveItem([FromBody] CartRemoveRequest request)
        {
            _cartService.RemoveFromCart(request.ItemId);
            var newTotal = _cartService.GetTotal();
            return Json(new { success = true, total = newTotal });
        }

        [HttpGet]
        [Route("checkout")]
        public IActionResult Checkout()
        {
            var cartItems = _cartService.GetCartItems();
            var checkoutViewModel = new CheckoutViewModel
            {
                CartItems = cartItems,
                // Add other required properties for CheckoutViewModel
            };
            return View(checkoutViewModel);
        }

        [HttpPost]
        [Route("process-checkout")]
        public IActionResult ProcessCheckout([FromForm] CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            _cartService.ClearCart();
            return RedirectToAction("OrderConfirmation");
        }

        [HttpGet]
        [Route("confirmation")]
        public IActionResult OrderConfirmation()
        {
            return View();
        }
    }
}
