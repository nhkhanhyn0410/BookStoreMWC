using Microsoft.EntityFrameworkCore;
using BookStoreMVC.Data;
using BookStoreMVC.Models.Entities;
using BookStoreMVC.Models.ViewModels;

namespace BookStoreMVC.Services
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<Book?> GetBookByIdAsync(int id);
        Task<Book?> GetBookByIdWithReviewsAsync(int id);
        Task<(IEnumerable<Book> Books, int TotalCount)> GetBooksAsync(BookListViewModel model);
        Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
        Task<IEnumerable<Book>> GetBooksByCategoryAsync(int categoryId);
        Task<IEnumerable<Book>> GetFeaturedBooksAsync(int count = 8);
        Task<IEnumerable<Book>> GetNewBooksAsync(int count = 8);
        Task<IEnumerable<Book>> GetBestSellersAsync(int count = 8);
        Task<IEnumerable<Book>> GetRelatedBooksAsync(int bookId, int count = 4);
        Task<Book> CreateBookAsync(Book book);
        Task<Book> UpdateBookAsync(Book book);
        Task<bool> DeleteBookAsync(int id);
        Task<bool> BookExistsAsync(int id);
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
    }

    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookService> _logger;

        public BookService(ApplicationDbContext context, ILogger<BookService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .Where(b => b.IsActive)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);
        }

        public async Task<Book?> GetBookByIdWithReviewsAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);
        }

        public async Task<(IEnumerable<Book> Books, int TotalCount)> GetBooksAsync(BookListViewModel model)
        {
            var query = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .Where(b => b.IsActive);

            // Apply filters
            if (model.CategoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == model.CategoryId.Value);
            }

            if (!string.IsNullOrEmpty(model.SearchIerm))
            {
                query = query.Where(b =>
                    b.Title.Contains(model.SearchIerm) ||
                    b.Author.Contains(model.SearchIerm) ||
                    b.Description!.Contains(model.SearchIerm));
            }

            if (model.MinPrice.HasValue)
            {
                query = query.Where(b => b.FinalPrice >= model.MinPrice.Value);
            }

            if (model.MaxPrice.HasValue)
            {
                query = query.Where(b => b.FinalPrice <= model.MaxPrice.Value);
            }

            if (model.InStock == true)
            {
                query = query.Where(b => b.StockQuantity > 0);
            }

            if (model.MinRating.HasValue)
            {
                query = query.Where(b => b.Reviews.Any() && b.Reviews.Average(r => r.Rating) >= model.MinRating.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = model.SortBy.ToLower() switch
            {
                "author" => model.SortOrder == "desc"
                    ? query.OrderByDescending(b => b.Author)
                    : query.OrderBy(b => b.Author),
                "price" => model.SortOrder == "desc"
                    ? query.OrderByDescending(b => b.FinalPrice)
                    : query.OrderBy(b => b.FinalPrice),
                "rating" => model.SortOrder == "desc"
                    ? query.OrderByDescending(b => b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0)
                    : query.OrderBy(b => b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0),
                "newest" => query.OrderByDescending(b => b.CreatedAt),
                "popularity" => query.OrderByDescending(b => b.Reviews.Count),
                _ => model.SortOrder == "desc"
                    ? query.OrderByDescending(b => b.Title)
                    : query.OrderBy(b => b.Title)
            };

            // Apply pagination
            var books = await query
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();

            return (books, totalCount);
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Enumerable.Empty<Book>();

            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .Where(b => b.IsActive && (
                    b.Title.Contains(searchTerm) ||
                    b.Author.Contains(searchTerm) ||
                    b.Description!.Contains(searchTerm) ||
                    b.Category.NameCategory.Contains(searchTerm)))
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksByCategoryAsync(int categoryId)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .Where(b => b.CategoryId == categoryId && b.IsActive)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetFeaturedBooksAsync(int count = 8)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .Where(b => b.IsActive && b.HasDiscount)
                .OrderByDescending(b => b.DiscountPercentage)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetNewBooksAsync(int count = 8)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBestSellersAsync(int count = 8)
        {
            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.OrderItems.Sum(oi => oi.Quantity))
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetRelatedBooksAsync(int bookId, int count = 4)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return Enumerable.Empty<Book>();

            return await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .Where(b => b.IsActive && b.Id != bookId && b.CategoryId == book.CategoryId)
                .OrderBy(b => Guid.NewGuid()) // Random order
                .Take(count)
                .ToListAsync();
        }

        public async Task<Book> CreateBookAsync(Book book)
        {
            try
            {
                book.CreatedAt = DateTime.UtcNow;
                book.UpdatedAt = DateTime.UtcNow;

                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Book created successfully: {BookTitle}", book.Title);
                return book;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book: {BookTitle}", book.Title);
                throw;
            }
        }

        public async Task<Book> UpdateBookAsync(Book book)
        {
            try
            {
                book.UpdatedAt = DateTime.UtcNow;

                _context.Books.Update(book);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Book updated successfully: {BookId}", book.Id);
                return book;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book: {BookId}", book.Id);
                throw;
            }
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null) return false;

                // Soft delete
                book.IsActive = false;
                book.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Book soft deleted: {BookId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book: {BookId}", id);
                return false;
            }
        }

        public async Task<bool> BookExistsAsync(int id)
        {
            return await _context.Books.AnyAsync(b => b.Id == id && b.IsActive);
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.NameCategory)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }
    }
}