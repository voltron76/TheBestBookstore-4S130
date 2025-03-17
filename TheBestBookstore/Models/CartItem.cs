using System;
using System.ComponentModel.DataAnnotations;
using TheBestBookstore.Models;

namespace TheBestBookstore.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string CartId { get; set; } = string.Empty;
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime DateCreated { get; set; }
        public virtual Book Book { get; set; } = null!;
    }
}
