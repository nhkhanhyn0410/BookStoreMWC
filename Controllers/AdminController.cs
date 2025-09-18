using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreMVC.Data;
using BookStoreMVC.Models.Entities;
using BookStoreMVC.Services;

namespace BookStoreMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookService _bookService;
        private readonly IOrderService _orderService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            IBookService bookService,
            IOrderService orderService,
            ILogger<AdminController> logger)
        {
            _context = context;
            _bookService = bookService;
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var stats = await GetDashboardStatsAsync();
                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                return View();
            }
        }

        public async Task<IActionResult> Books(int page = 1, string search = "")
        {
            try
            {
                var pageSize = 10;
                var query = _context.Books.Include(b => b.Category).AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(b => b.Title.Contains(search) || b.Author.Contains(search));
                }

                var totalBooks = await query.CountAsync();
                var books = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalBooks / pageSize);
                ViewBag.Search = search;

                return View(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading books management page");
                return View(new List<Book>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateBook()
        {
            try
            {
                ViewBag.Categories = await _bookService.GetCategoriesAsync();
                return View(new Book());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create book page");
                return RedirectToAction(nameof(Books));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBook(Book book)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = await _bookService.GetCategoriesAsync();
                    return View(book);
                }

                await _bookService.CreateBookAsync(book);
                TempData["SuccessMessage"] = "Book created successfully!";
                return RedirectToAction(nameof(Books));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the book.");
                ViewBag.Categories = await _bookService.GetCategoriesAsync();
                return View(book);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditBook(int id)
        {
            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null)
                {
                    return NotFound();
                }

                ViewBag.Categories = await _bookService.GetCategoriesAsync();
                return View(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit book page for book {BookId}", id);
                return RedirectToAction(nameof(Books));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(Book book)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = await _bookService.GetCategoriesAsync();
                    return View(book);
                }

                await _bookService.UpdateBookAsync(book);
                TempData["SuccessMessage"] = "Book updated successfully!";
                return RedirectToAction(nameof(Books));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book {BookId}", book.Id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the book.");
                ViewBag.Categories = await _bookService.GetCategoriesAsync();
                return View(book);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                var success = await _bookService.DeleteBookAsync(id);

                if (success)
                {
                    return Json(new { success = true, message = "Book deleted successfully" });
                }

                return Json(new { success = false, message = "Book not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book {BookId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the book" });
            }
        }

        public async Task<IActionResult> Orders(int page = 1, string status = "")
        {
            try
            {
                var pageSize = 15;
                var query = _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
                {
                    query = query.Where(o => o.Status == orderStatus);
                }

                var totalOrders = await query.CountAsync();
                var orders = await query
                    .OrderByDescending(o => o.OrderDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
                ViewBag.StatusFilter = status;
                ViewBag.OrderStatuses = Enum.GetValues<OrderStatus>();

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders management page");
                return View(new List<Order>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            try
            {
                var success = await _orderService.UpdateOrderStatusAsync(orderId, status);

                if (success)
                {
                    return Json(new { success = true, message = "Order status updated successfully" });
                }

                return Json(new { success = false, message = "Order not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for order {OrderId}", orderId);
                return Json(new { success = false, message = "An error occurred while updating the order status" });
            }
        }

        public async Task<IActionResult> Users(int page = 1, string search = "")
        {
            try
            {
                var pageSize = 15;
                var query = _context.Users.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u => u.FirstName.Contains(search) ||
                                           u.LastName.Contains(search) ||
                                           u.Email!.Contains(search));
                }

                var totalUsers = await query.CountAsync();
                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
                ViewBag.Search = search;

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users management page");
                return View(new List<User>());
            }
        }

        // AJAX endpoints for dashboard
        [HttpGet]
        public async Task<IActionResult> GetSalesData(int days = 30)
        {
            try
            {
                var endDate = DateTime.UtcNow.Date;
                var startDate = endDate.AddDays(-days);

                var salesData = await _context.Orders
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status != OrderStatus.Cancelled)
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new
                    {
                        date = g.Key.ToString("yyyy-MM-dd"),
                        sales = g.Sum(o => o.Total),
                        orders = g.Count()
                    })
                    .OrderBy(x => x.date)
                    .ToListAsync();

                return Json(new { success = true, data = salesData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales data");
                return Json(new { success = false, message = "Error loading sales data" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTopSellingBooks(int count = 10)
        {
            try
            {
                var topBooks = await _context.OrderItems
                    .Include(oi => oi.Book)
                    .GroupBy(oi => new { oi.BookId, oi.BookTitle })
                    .Select(g => new
                    {
                        bookId = g.Key.BookId,
                        title = g.Key.BookTitle,
                        totalSold = g.Sum(oi => oi.Quantity),
                        revenue = g.Sum(oi => oi.Total)
                    })
                    .OrderByDescending(x => x.totalSold)
                    .Take(count)
                    .ToListAsync();

                return Json(new { success = true, data = topBooks });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top selling books");
                return Json(new { success = false, message = "Error loading top selling books" });
            }
        }

        private async Task<object> GetDashboardStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            var stats = new
            {
                TotalBooks = await _context.Books.Where(b => b.IsActive).CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalUsers = await _context.Users.Where(u => u.IsActive).CountAsync(),
                TotalRevenue = await _context.Orders
                    .Where(o => o.Status != OrderStatus.Cancelled)
                    .SumAsync(o => o.Total),

                TodayOrders = await _context.Orders
                    .Where(o => o.OrderDate.Date == today)
                    .CountAsync(),
                TodayRevenue = await _context.Orders
                    .Where(o => o.OrderDate.Date == today && o.Status != OrderStatus.Cancelled)
                    .SumAsync(o => o.Total),

                ThisMonthOrders = await _context.Orders
                    .Where(o => o.OrderDate >= thisMonth)
                    .CountAsync(),
                ThisMonthRevenue = await _context.Orders
                    .Where(o => o.OrderDate >= thisMonth && o.Status != OrderStatus.Cancelled)
                    .SumAsync(o => o.Total),

                LastMonthOrders = await _context.Orders
                    .Where(o => o.OrderDate >= lastMonth && o.OrderDate < thisMonth)
                    .CountAsync(),
                LastMonthRevenue = await _context.Orders
                    .Where(o => o.OrderDate >= lastMonth && o.OrderDate < thisMonth && o.Status != OrderStatus.Cancelled)
                    .SumAsync(o => o.Total),

                PendingOrders = await _context.Orders
                    .Where(o => o.Status == OrderStatus.Pending)
                    .CountAsync(),
                LowStockBooks = await _context.Books
                    .Where(b => b.IsActive && b.StockQuantity <= 5)
                    .CountAsync()
            };

            return stats;
        }
    }
}