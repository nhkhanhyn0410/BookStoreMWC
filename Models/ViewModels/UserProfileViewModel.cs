using System.ComponentModel.DataAnnotations;

namespace BookStoreMVC.Models.ViewModels
{
    public class UserProfileViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        // Profile statistics
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public int ReviewsCount { get; set; }
        public int WishlistCount { get; set; }
        public DateTime MemberSince { get; set; }

        // Recent activity
        public IEnumerable<OrderViewModel> RecentOrders { get; set; } = new List<OrderViewModel>();
        public IEnumerable<ReviewViewModel> RecentReviews { get; set; } = new List<ReviewViewModel>();
    }
}