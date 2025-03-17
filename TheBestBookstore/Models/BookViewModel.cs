using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace TheBestBookstore.Models
{
    public class BookViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [StringLength(20)]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public IFormFile? ImageFile { get; set; }

        [Url]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime Published { get; set; }

        [Range(1, 10000)]
        public int Pages { get; set; }

        [Required]
        [Range(0.01, 1000)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Best Seller")]
        public bool IsBestSeller { get; set; }
    }
}
