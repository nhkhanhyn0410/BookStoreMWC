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
        public int Id
        { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Size { get; set; }

        [Column(TypeName = "text")]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [StringLength(20)]
        public string? CoverType { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DiscountPrice { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public int StockQuantity { get; set; }

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        [StringLength(100)]
        public string? Publisher { get; set; }

        public DateTime? PublisheDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page count must be at least 1.")]
        public int? PageCount { get; set; }

        [StringLength(50)]
        public string? Language { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        // Navigation properties
        public virtual ICollection<CartItem> CartItem { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<WishListItem> WishlistItems { get; set; } = new List<WishListItem>();

        // Computed properties
        [NotMapped]
        public decimal FinalPrice => DiscountPrice ?? Price;

        [NotMapped]
        public bool HasDiscount => DiscountPrice.HasValue && DiscountPrice < Price;

        [NotMapped]
        public decimal DiscountPercentage => HasDiscount ? Math.Round(((Price - DiscountPrice!.Value) / Price) * 100, 0) : 0;

        [NotMapped]
        public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;

        [NotMapped]
        public int ReviewCount => Reviews.Count;

        [NotMapped]
        public bool IsInStock => StockQuantity > 0;

        [NotMapped]
        public string StockStatus => StockQuantity switch
        {
            0 => "Out of Stock",
            < 5 => "Low Stock",
            _ => "In Stock"
        };


    }
}