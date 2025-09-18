using BookStoreMVC.Models.Entities;

namespace BookStoreMVC.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Statistics
        public int TotalUsers { get; set; }
        public int TotalBooks { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCategories { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int LowStockBooks { get; set; }
        public int UnapprovedReviews { get; set; }

        // Recent activity
        public IEnumerable<OrderViewModel> RecentOrders { get; set; } = new List<OrderViewModel>();
        public IEnumerable<ReviewViewModel> RecentReviews { get; set; } = new List<ReviewViewModel>();
        public IEnumerable<User> NewUsers { get; set; } = new List<User>();
        public IEnumerable<BookViewModel> LowStockItems { get; set; } = new List<BookViewModel>();

        // Charts data
        public Dictionary<string, decimal> MonthlyRevenue { get; set; } = new();
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
        public Dictionary<string, int> BooksByCategory { get; set; } = new();
        public Dictionary<string, int> UserRegistrations { get; set; } = new();
    }

    public class UserDashboardViewModel
    {
        public User User { get; set; } = new();
        public int CartItemsCount { get; set; }
        public int WishlistItemsCount { get; set; }
        public int PendingOrdersCount { get; set; }
        public decimal TotalSpent { get; set; }

        // Recent activity
        public IEnumerable<OrderViewModel> RecentOrders { get; set; } = new List<OrderViewModel>();
        public IEnumerable<BookViewModel> RecommendedBooks { get; set; } = new List<BookViewModel>();
        public IEnumerable<BookViewModel> ContinueReading { get; set; } = new List<BookViewModel>();
        public IEnumerable<ReviewViewModel> MyRecentReviews { get; set; } = new List<ReviewViewModel>();
    }
}