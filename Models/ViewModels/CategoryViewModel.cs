using System.ComponentModel.DataAnnotations;
using BookStoreMVC.Models.Entities;

namespace BookStoreMVC.Models.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot be longer than 100 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Parent Category")]
        public int? ParentCategoryId { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Image URL")]
        [StringLength(255, ErrorMessage = "Image URL cannot be longer than 255 characters")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Category? ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public ICollection<Book> Books { get; set; } = new List<Book>();
        public IEnumerable<Category> AvailableParentCategories { get; set; } = new List<Category>();

        // Calculated properties
        public int BookCount => Books.Count(b => b.IsActive);
        public bool HasSubCategories => SubCategories.Any();
        public string FullPath => ParentCategory != null ? $"{ParentCategory.Name} > {Name}" : Name;
    }

    public class CategoryListViewModel
    {
        public IEnumerable<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
        public string? SearchTerm { get; set; }
        public bool ShowInactiveCategories { get; set; }
        public string SortBy { get; set; } = "name";
        public string SortOrder { get; set; } = "asc";

        public Dictionary<string, string> SortOptions => new()
        {
            {"name", "Name A-Z"},
            {"name_desc", "Name Z-A"},
            {"bookcount", "Book Count (Low to High)"},
            {"bookcount_desc", "Book Count (High to Low)"},
            {"created", "Created Date (Oldest)"},
            {"created_desc", "Created Date (Newest)"}
        };
    }
}