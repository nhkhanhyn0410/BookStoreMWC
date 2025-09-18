using Microsoft.AspNetCore.Mvc;
using BookStoreMVC.Models.ViewModels;
using BookStoreMVC.Services;

namespace BookStoreMVC.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IBookService bookService, ILogger<BooksController> logger)
        {
            _bookService = bookService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(BookListViewModel model)
        {
            try
            {
                // Set default values
                model.PageSize = Math.Max(1, Math.Min(model.PageSize, 50)); // Limit page size
                model.PageNumber = Math.Max(1, model.PageNumber);

                // Get books with filters and pagination
                var (books, totalCount) = await _bookService.GetBooksAsync(model);

                model.Books = books;
                model.TotalCount = totalCount;
                model.Categories = await _bookService.GetCategoriesAsync();

                // Add breadcrumb data
                ViewBag.CategoryName = model.CategoryId.HasValue
                    ? (await _bookService.GetCategoryByIdAsync(model.CategoryId.Value))?.NameCategory
                    : null;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading books index page");
                return View(new BookListViewModel());
            }
        }

        public async Task<IActionResult> Details(int id, string? title)
        {
            try
            {
                var book = await _bookService.GetBookByIdWithReviewsAsync(id);
                if (book == null)
                {
                    return NotFound();
                }

                // SEO: Redirect if title doesn't match
                var expectedTitle = book.Title.ToLower().Replace(" ", "-");
                if (!string.IsNullOrEmpty(title) && title != expectedTitle)
                {
                    return RedirectToAction(nameof(Details), new { id, title = expectedTitle });
                }

                // Get related books
                var relatedBooks = await _bookService.GetRelatedBooksAsync(id, 4);
                ViewBag.RelatedBooks = relatedBooks;

                return View(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading book details for ID: {BookId}", id);
                return NotFound();
            }
        }

        public async Task<IActionResult> Category(int id, string? name)
        {
            try
            {
                var category = await _bookService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                var model = new BookListViewModel
                {
                    CategoryId = id,
                    PageSize = 12
                };

                var (books, totalCount) = await _bookService.GetBooksAsync(model);

                model.Books = books;
                model.TotalCount = totalCount;
                model.Categories = await _bookService.GetCategoriesAsync();

                ViewBag.Category = category;
                ViewBag.PageTitle = $"{category.NameCategory} Books";

                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category page for ID: {CategoryId}", id);
                return NotFound();
            }
        }

        public async Task<IActionResult> Search(string searchTerm, int page = 1)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    return RedirectToAction(nameof(Index));
                }

                var model = new BookListViewModel
                {
                    SearchIerm = searchTerm,
                    PageNumber = page,
                    PageSize = 12
                };

                var (books, totalCount) = await _bookService.GetBooksAsync(model);

                model.Books = books;
                model.TotalCount = totalCount;
                model.Categories = await _bookService.GetCategoriesAsync();

                ViewBag.PageTitle = $"Search Results for '{searchTerm}'";

                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching books with term: {SearchTerm}", searchTerm);
                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX endpoints
        [HttpGet]
        public async Task<IActionResult> FilterBooks(BookListViewModel model)
        {
            try
            {
                var (books, totalCount) = await _bookService.GetBooksAsync(model);

                var result = new
                {
                    success = true,
                    books = books.Select(b => new
                    {
                        id = b.Id,
                        title = b.Title,
                        author = b.Author,
                        price = b.Price,
                        discountPrice = b.DiscountPrice,
                        finalPrice = b.FinalPrice,
                        imageUrl = b.ImageUrl,
                        rating = b.AverageRating,
                        reviewCount = b.ReviewCount,
                        hasDiscount = b.HasDiscount,
                        isInStock = b.IsInStock,
                        stockStatus = b.StockStatus
                    }),
                    totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / model.PageSize)
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering books");
                return Json(new { success = false, message = "Error loading books" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> QuickView(int id)
        {
            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null)
                {
                    return NotFound();
                }

                return PartialView("_QuickViewPartial", book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading quick view for book ID: {BookId}", id);
                return NotFound();
            }
        }
    }
}