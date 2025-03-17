using System.ComponentModel.DataAnnotations;

namespace TheBestBookstore.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public decimal Price { get; set; }
        
        [Required] 
        public string Description { get; set; }

        public Product(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public Product() 
        { 
            Name = string.Empty;
            Description = string.Empty;
        }
    }
}
