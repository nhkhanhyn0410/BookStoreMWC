using System.ComponentModel.DataAnnotations;

namespace BookStoreMVC.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? NameCategory { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();

        // Computed properties
        public int BookCount => Books.Count(b => b.IsActive);
    }
}