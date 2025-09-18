using BookStoreMVC.Models.Entities;

namespace BookStoreMVC.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Book> FeaturedBooks { get; set; } = new();
        public List<Book> NewBooks { get; set; } = new();
        public List<Book> BestSellers { get; set; } = new();
        public List<Category> PopularCategories { get; set; } = new();

        public int TotalBooks { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }

        public List<Review> RecentReviews { get; set; } = new();
    }
}