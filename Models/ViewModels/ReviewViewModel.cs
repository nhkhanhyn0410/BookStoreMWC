using System.ComponentModel.DataAnnotations;
using BookStoreMVC.Models.Entities;

namespace BookStoreMVC.Models.ViewModels
{
    public class ReviewViewModel
    {
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(100)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public List<Review> ExistingReviews { get; set; } = new();

        public bool CanReview { get; set; }
        public bool HasPurchased { get; set; }
        public bool AlreadyReviewed { get; set; }
    }
}