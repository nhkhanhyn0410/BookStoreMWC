using System.ComponentModel.DataAnnotations;
using BookStoreMVC.Models.Entities;

namespace BookStoreMVC.Models.ViewModels
{
    public class ReviewCreateViewModel
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        [Display(Name = "Rating")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment cannot be longer than 1000 characters")]
        [Display(Name = "Review Comment")]
        public string? Comment { get; set; }

        public BookViewModel Book { get; set; } = new();
    }

    public class ReviewViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int BookId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = new();
        public Book Book { get; set; } = new();

        // Calculated properties
        public string UserDisplayName => User?.Name ?? "Anonymous";
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.UtcNow - CreatedAt;
                if (timeSpan.TotalDays >= 365)
                    return $"{(int)(timeSpan.TotalDays / 365)} year(s) ago";
                if (timeSpan.TotalDays >= 30)
                    return $"{(int)(timeSpan.TotalDays / 30)} month(s) ago";
                if (timeSpan.TotalDays >= 1)
                    return $"{(int)timeSpan.TotalDays} day(s) ago";
                if (timeSpan.TotalHours >= 1)
                    return $"{(int)timeSpan.TotalHours} hour(s) ago";
                return "Just now";
            }
        }
        public string StarDisplay => new string('★', Rating) + new string('☆', 5 - Rating);
    }

    public class ReviewListViewModel
    {
        public IEnumerable<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();
        public int? BookId { get; set; }
        public int? Rating { get; set; }
        public bool ShowUnapproved { get; set; }
        public string SortBy { get; set; } = "newest";

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        // Statistics
        public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;
        public int[] RatingDistribution => Enumerable.Range(1, 5)
            .Select(i => Reviews.Count(r => r.Rating == i))
            .ToArray();

        public Dictionary<string, string> SortOptions => new()
        {
            {"newest", "Newest First"},
            {"oldest", "Oldest First"},
            {"highest_rating", "Highest Rating"},
            {"lowest_rating", "Lowest Rating"}
        };
    }
}