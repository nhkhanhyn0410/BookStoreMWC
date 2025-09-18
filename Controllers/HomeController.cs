using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreMVC.Data;
using BookStoreMVC.Models;
using BookStoreMVC.Models.ViewModels;
using BookStoreMVC.Services;
using System.Diagnostics;

namespace BookStoreMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBookService _bookService;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            IBookService bookService,
            ApplicationDbContext context)
        {
            _logger = logger;
            _bookService = bookService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var model = new HomeViewModel
                {
                    FeaturedBooks = (await _bookService.GetFeaturedBooksAsync(8)).ToList(),
                    NewBooks = (await _bookService.GetNewBooksAsync(8)).ToList(),
                    BestSellers = (await _bookService.GetBestSellersAsync(8)).ToList(),
                    PopularCategories = (await _bookService.GetCategoriesAsync()).Take(6).ToList(),

                    TotalBooks = await _context.Books.Where(b => b.IsActive).CountAsync(),
                    TotalCategories = await _context.Categories.Where(c => c.IsActive).CountAsync(),
                    TotalUsers = await _context.Users.Where(u => u.IsActive).CountAsync(),

                    RecentReviews = await _context.Reviews
                        .Include(r => r.User)
                        .Include(r => r.Book)
                        .Where(r => r.IsApproved)
                        .OrderByDescending(r => r.CreatedAt)
                        .Take(6)
                        .ToListAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                return View(new HomeViewModel());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        // API endpoints for AJAX calls
        [HttpGet]
        public async Task<IActionResult> GetFeaturedBooks()
        {
            try
            {
                var books = await _bookService.GetFeaturedBooksAsync(4);
                return Json(new { success = true, data = books });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured books");
                return Json(new { success = false, message = "Error loading featured books" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
            {
                return Json(new { suggestions = new List<object>() });
            }

            try
            {
                var books = await _bookService.SearchBooksAsync(term);
                var suggestions = books.Take(5).Select(b => new
                {
                    id = b.Id,
                    title = b.Title,
                    author = b.Author,
                    price = b.FinalPrice,
                    imageUrl = b.ImageUrl
                }).ToList();

                return Json(new { suggestions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions for term: {Term}", term);
                return Json(new { suggestions = new List<object>() });
            }
        }
    }
}