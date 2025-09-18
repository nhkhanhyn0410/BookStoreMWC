using System.ComponentModel.DataAnnotations;
using BookStoreMVC.Models.Entities;

namespace BookStoreMVC.Models.ViewModels
{
    public class BookListViewModel
    {
        public IEnumerable<Book> Books { get; set; } = new List<Book>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        // Filtering
        public int? CategoryId { get; set; }
        public string? SearchIerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinRating { get; set; }
        public bool InStock { get; set; }

        // Sorting
        public string SortBy { get; set; } = "title";
        public string SortOrder { get; set; } = "asc";

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        // Display options
        public string ViewMode { get; set; } = "gird"; // gird or list

        public Dictionary<string, string> SortOptions => new()
        {
            {"title", "Title"},
            { "author", "Author" },
            { "price", "Price" },
            { "rating", "Rating" },
            { "newest", "Newest" },
            { "popularity", "Popularity" }
        };
    }
}