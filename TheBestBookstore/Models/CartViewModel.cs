using System.Collections.Generic;

namespace TheBestBookstore.Models
{
    public class CartViewModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal CartTotal { get; set; }
    }
}
