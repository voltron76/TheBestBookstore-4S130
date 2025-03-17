using System.Collections.Generic;
using TheBestBookstore.Models;

namespace TheBestBookstore.Services
{
    public interface ICartService
    {
        List<CartItem> GetCartItems();
        decimal GetTotal();
        void AddToCart(int bookId, int quantity);
        void UpdateQuantity(int itemId, int quantity);
        void RemoveFromCart(int itemId);
        void ClearCart();
        Book? GetBook(int bookId);
    }
}
