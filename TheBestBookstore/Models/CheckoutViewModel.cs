using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TheBestBookstore.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Please enter your name")]
        [Display(Name = "Name")]
        public string CustomerName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Please enter your email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Please enter your address")]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;
        
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        
        public decimal CartTotal { get; set; }
    }
}
