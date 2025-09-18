// Models/Entities/Book.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreMVC.Models.Entities
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DiscountPrice { get; set; }

        public int StockQuantity { get; set; }

        public int CategoryId { get; set; }

        [StringLength(100)]
        public string? Publisher { get; set; }

        public DateTime? PublishDate { get; set; }

        public int? PageCount { get; set; }

        [StringLength(50)]
        public string? Language { get; set; }

        // Image fields
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(255)]
        public string? ImageFileName { get; set; }

        [StringLength(100)]
        public string? ImageContentType { get; set; }

        public long? ImageFileSize { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<WishListItem> WishlistItems { get; set; } = new List<WishListItem>();

        // Helper properties for image handling
        [NotMapped]
        public string DefaultImageUrl => ImageUrl ?? "/images/books/default-book.jpg";

        [NotMapped]
        public bool HasImage => !string.IsNullOrEmpty(ImageUrl);

        [NotMapped]
        public string ImageAlt => $"Cover image of {Title} by {Author}";

        [NotMapped]
        public string FormattedFileSize => ImageFileSize.HasValue ?
            FormatFileSize(ImageFileSize.Value) : "Unknown";

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}