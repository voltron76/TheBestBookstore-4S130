using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TheBestBookstore.Models
{
    public class Cart
    {
        [Key]
        public string CartId { get; set; } = string.Empty;
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
