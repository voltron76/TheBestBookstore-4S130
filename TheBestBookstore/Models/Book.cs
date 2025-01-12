using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheBestBookstore.Models
{
    /// <summary>
    /// The book object - an entry in the books catalog
    /// </summary>
    public class Book
    {
        /// <summary>
        /// Book identifier in the system
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Title of the book
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// First and last names of the author of the book
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The date the book was published
        /// </summary>
        public DateTime Published { get; set; }

        /// <summary>
        /// Number of pages
        /// </summary>
        public int Pages { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public Category Category { get; set; }
    }
}
