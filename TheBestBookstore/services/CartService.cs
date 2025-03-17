using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TheBestBookstore.Data;
using TheBestBookstore.Models;

namespace TheBestBookstore.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string CartId { get; set; }

        public CartService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            CartId = GetCartId();
        }

        private string GetCartId()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            string cartId = session?.GetString("CartId") ?? string.Empty;

            if (string.IsNullOrEmpty(cartId))
            {
                cartId = Guid.NewGuid().ToString();
                session?.SetString("CartId", cartId);
            }

            return cartId;
        }

        public List<CartItem> GetCartItems()
        {
            return _context.CartItems
                .Include(c => c.Book)
                .Where(c => c.CartId == CartId)
                .ToList();
        }

        public decimal GetTotal()
        {
            return GetCartItems().Sum(item => item.Quantity * item.UnitPrice);
        }

        public void AddToCart(int bookId, int quantity)
        {
            var book = _context.Books.Find(bookId);
            if (book == null) return;

            var cartItem = _context.CartItems
                .FirstOrDefault(c => c.CartId == CartId && c.BookId == bookId);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartId = CartId,
                    BookId = bookId,
                    Quantity = quantity,
                    UnitPrice = book.Price,
                    DateCreated = DateTime.Now
                };
                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
            }
            _context.SaveChanges();
        }

        public void UpdateQuantity(int itemId, int quantity)
        {
            var cartItem = _context.CartItems.Find(itemId);
            if (cartItem != null && cartItem.CartId == CartId)
            {
                if (quantity > 0)
                {
                    cartItem.Quantity = quantity;
                    _context.SaveChanges();
                }
                else
                {
                    RemoveFromCart(itemId);
                }
            }
        }

        public void RemoveFromCart(int itemId)
        {
            var cartItem = _context.CartItems.Find(itemId);
            if (cartItem != null && cartItem.CartId == CartId)
            {
                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();
            }
        }

        public void ClearCart()
        {
            var cartItems = _context.CartItems.Where(c => c.CartId == CartId);
            _context.CartItems.RemoveRange(cartItems);
            _context.SaveChanges();
        }

        public Book? GetBook(int bookId)
        {
            return _context.Books.Find(bookId);
        }
    }
}
