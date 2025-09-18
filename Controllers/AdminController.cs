// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookStoreMVC.Models.Entities;
using BookStoreMVC.Models.ViewModels;
using BookStoreMVC.Services;

namespace BookStoreMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IBookService _bookService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IDashboardService dashboardService,
            IBookService bookService,
            IOrderService orderService,
            IUserService userService,
            IFileUploadService fileUploadService,
            ILogger<AdminController> logger)
        {
            _dashboardService = dashboardService;
            _bookService = bookService;
            _orderService = orderService;
            _userService = userService;
            _logger = logger;
        }

        #region Dashboard

        [Route("")]
        [Route("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var model = await _dashboardService.GetAdminDashboardAsync();
                ViewBag.PageTitle = "Admin Dashboard";
                ViewBag.ActiveMenu = "Dashboard";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                return View(new AdminDashboardViewModel());
            }
        }

        [HttpGet]
        [Route("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return Json(new { success = false, message = "Error loading statistics" });
            }
        }

        #endregion

        #region Books Management

        [Route("books")]
        public async Task<IActionResult> Books(BookListViewModel model)
        {
            try
            {
                var (books, totalCount) = await _bookService.GetBooksAsync(model);

                model.Books = books;
                model.TotalCount = totalCount;
                model.Categories = await _bookService.GetCategoriesAsync();

                ViewBag.PageTitle = "Manage Books";
                ViewBag.ActiveMenu = "Books";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading books management page");
                return View(new BookListViewModel());
            }
        }

        [HttpGet]
        [Route("books/create")]
        public async Task<IActionResult> CreateBook()
        {
            try
            {
                var model = new BookViewModel
                {
                    Categories = (await _bookService.GetCategoriesAsync()).ToList()
                };

                ViewBag.PageTitle = "Create Book";
                ViewBag.ActiveMenu = "Books";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create book page");
                return RedirectToAction(nameof(Books));
            }
        }


        #region Books Management with Image Support
        [HttpPost]
        [Route("books/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBook(BookViewModel model)
        {
            try
            {
                // Validate image file if provided
                if (model.ImageFile != null && !_fileUploadService.IsValidImageFile(model.ImageFile))
                {
                    ModelState.AddModelError("ImageFile", "Please upload a valid image file (JPG, PNG, GIF, WebP) smaller than 5MB.");
                }

                if (!ModelState.IsValid)
                {
                    model.Categories = (await _bookService.GetCategoriesAsync()).ToList();
                    ViewBag.ActiveMenu = "Books";
                    return View(model);
                }

                await _bookService.CreateBookAsync(model, model.ImageFile);
                TempData["SuccessMessage"] = "Book created successfully!";
                return RedirectToAction(nameof(Books));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the book.");
                model.Categories = (await _bookService.GetCategoriesAsync()).ToList();
                ViewBag.ActiveMenu = "Books";
                return View(model);
            }
        }

        [HttpGet]
        [Route("books/edit/{id:int}")]
        public async Task<IActionResult> EditBook(int id)
        {
            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null)
                {
                    return NotFound();
                }

                book.Categories = (await _bookService.GetCategoriesAsync()).ToList();
                ViewBag.PageTitle = $"Edit Book: {book.Title}";
                ViewBag.ActiveMenu = "Books";
                return View(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit book page for book {BookId}", id);
                return RedirectToAction(nameof(Books));
            }
        }

        [HttpPost]
        [Route("books/edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(BookViewModel model)
        {
            try
            {
                // Validate image file if provided
                if (model.ImageFile != null && !_fileUploadService.IsValidImageFile(model.ImageFile))
                {
                    ModelState.AddModelError("ImageFile", "Please upload a valid image file (JPG, PNG, GIF, WebP) smaller than 5MB.");
                }

                if (!ModelState.IsValid)
                {
                    model.Categories = (await _bookService.GetCategoriesAsync()).ToList();
                    ViewBag.ActiveMenu = "Books";
                    return View(model);
                }

                await _bookService.UpdateBookAsync(model, model.ImageFile);
                TempData["SuccessMessage"] = "Book updated successfully!";
                return RedirectToAction(nameof(Books));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book {BookId}", model.Id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the book.");
                model.Categories = (await _bookService.GetCategoriesAsync()).ToList();
                ViewBag.ActiveMenu = "Books";
                return View(model);
            }
        }

        [HttpPost]
        [Route("books/upload-image/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadBookImage(int id, IFormFile imageFile)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    return Json(new { success = false, message = "Please select an image file." });
                }

                if (!_fileUploadService.IsValidImageFile(imageFile))
                {
                    return Json(new { success = false, message = "Please upload a valid image file (JPG, PNG, GIF, WebP) smaller than 5MB." });
                }

                var success = await _bookService.UpdateBookImageAsync(id, imageFile);

                if (success)
                {
                    // Get updated book to return new image URL
                    var book = await _bookService.GetBookByIdAsync(id);
                    return Json(new
                    {
                        success = true,
                        message = "Image uploaded successfully!",
                        imageUrl = book?.ImageUrl,
                        defaultImageUrl = book?.DefaultImageUrl
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to upload image. Please try again." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading book image for book {BookId}", id);
                return Json(new { success = false, message = "An error occurred while uploading the image." });
            }
        }

        [HttpPost]
        [Route("books/remove-image/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveBookImage(int id)
        {
            try
            {
                var success = await _bookService.RemoveBookImageAsync(id);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Image removed successfully!",
                        defaultImageUrl = "/images/books/default-book.jpg"
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to remove image or image not found." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing book image for book {BookId}", id);
                return Json(new { success = false, message = "An error occurred while removing the image." });
            }
        }

        #endregion

        #region Image Management Utilities

        [HttpGet]
        [Route("images/gallery")]
        public async Task<IActionResult> ImageGallery()
        {
            try
            {
                // Get all books with images for gallery view
                var booksModel = new BookListViewModel { PageSize = 100 };
                var (books, totalCount) = await _bookService.GetBooksAsync(booksModel);

                var imageGallery = books
                    .Where(b => b.HasImage)
                    .Select(b => new
                    {
                        BookId = b.Id,
                        BookTitle = b.Title,
                        ImageUrl = b.ImageUrl,
                        FileName = b.ImageFileName,
                        FileSize = b.FormattedFileSize,
                        UploadDate = "Unknown" // You can add this field to Book entity if needed
                    })
                    .ToList();

                ViewBag.PageTitle = "Image Gallery";
                ViewBag.ActiveMenu = "Images";
                return View(imageGallery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading image gallery");
                return View(new List<object>());
            }
        }

        [HttpPost]
        [Route("images/bulk-upload")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUploadImages(List<IFormFile> files)
        {
            try
            {
                var results = new List<object>();

                foreach (var file in files)
                {
                    if (_fileUploadService.IsValidImageFile(file))
                    {
                        var uploadResult = await _fileUploadService.UploadImageAsync(file, "books");
                        results.Add(new
                        {
                            fileName = file.FileName,
                            success = uploadResult.Success,
                            imageUrl = uploadResult.ImageUrl,
                            message = uploadResult.Success ? "Uploaded successfully" : uploadResult.ErrorMessage
                        });
                    }
                    else
                    {
                        results.Add(new
                        {
                            fileName = file.FileName,
                            success = false,
                            message = "Invalid file type or size"
                        });
                    }
                }

                return Json(new { success = true, results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk image upload");
                return Json(new { success = false, message = "An error occurred during bulk upload." });
            }
        }

        #endregion

        [HttpPost]
        [Route("books/delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                var success = await _bookService.DeleteBookAsync(id);

                if (success)
                {
                    return Json(new { success = true, message = "Book deleted successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Book not found or unable to delete." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book {BookId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the book." });
            }
        }

        #endregion

        #region Categories Management

        [Route("categories")]
        public async Task<IActionResult> Categories(CategoryListViewModel model)
        {
            try
            {
                var categories = await _bookService.GetCategoriesWithStatsAsync();
                model.Categories = categories;

                ViewBag.PageTitle = "Manage Categories";
                ViewBag.ActiveMenu = "Categories";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories management page");
                return View(new CategoryListViewModel());
            }
        }

        [HttpGet]
        [Route("categories/create")]
        public async Task<IActionResult> CreateCategory()
        {
            try
            {
                var model = new CategoryViewModel
                {
                    AvailableParentCategories = (await _bookService.GetCategoriesAsync()).ToList()
                };

                ViewBag.PageTitle = "Create Category";
                ViewBag.ActiveMenu = "Categories";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create category page");
                return RedirectToAction(nameof(Categories));
            }
        }

        [HttpPost]
        [Route("categories/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.AvailableParentCategories = (await _bookService.GetCategoriesAsync()).ToList();
                    ViewBag.ActiveMenu = "Categories";
                    return View(model);
                }

                await _bookService.CreateCategoryAsync(model);
                TempData["SuccessMessage"] = "Category created successfully!";
                return RedirectToAction(nameof(Categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the category.");
                model.AvailableParentCategories = (await _bookService.GetCategoriesAsync()).ToList();
                ViewBag.ActiveMenu = "Categories";
                return View(model);
            }
        }

        [HttpGet]
        [Route("categories/edit/{id:int}")]
        public async Task<IActionResult> EditCategory(int id)
        {
            try
            {
                var category = await _bookService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                category.AvailableParentCategories = (await _bookService.GetCategoriesAsync())
                    .Where(c => c.Id != id) // Exclude self from parent options
                    .ToList();

                ViewBag.PageTitle = $"Edit Category: {category.Name}";
                ViewBag.ActiveMenu = "Categories";
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit category page for category {CategoryId}", id);
                return RedirectToAction(nameof(Categories));
            }
        }

        [HttpPost]
        [Route("categories/edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(CategoryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.AvailableParentCategories = (await _bookService.GetCategoriesAsync())
                        .Where(c => c.Id != model.Id)
                        .ToList();
                    ViewBag.ActiveMenu = "Categories";
                    return View(model);
                }

                await _bookService.UpdateCategoryAsync(model);
                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction(nameof(Categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", model.Id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the category.");
                model.AvailableParentCategories = (await _bookService.GetCategoriesAsync())
                    .Where(c => c.Id != model.Id)
                    .ToList();
                ViewBag.ActiveMenu = "Categories";
                return View(model);
            }
        }

        [HttpPost]
        [Route("categories/delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var success = await _bookService.DeleteCategoryAsync(id);

                if (success)
                {
                    return Json(new { success = true, message = "Category deleted successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Category not found or unable to delete." });
                }
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                return Json(new { success = false, message = "An error occurred while deleting the category." });
            }
        }

        #endregion

        #region Orders Management

        [Route("orders")]
        public async Task<IActionResult> Orders(OrderListViewModel model)
        {
            try
            {
                var (orders, totalCount) = await _orderService.GetOrdersAsync(model);

                model.Orders = orders;
                model.TotalCount = totalCount;

                ViewBag.PageTitle = "Manage Orders";
                ViewBag.ActiveMenu = "Orders";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders management page");
                return View(new OrderListViewModel());
            }
        }

        [Route("orders/details/{id:int}")]
        public async Task<IActionResult> OrderDetails(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    return NotFound();
                }

                ViewBag.PageTitle = $"Order {order.OrderNumber}";
                ViewBag.ActiveMenu = "Orders";
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details for ID: {OrderId}", id);
                return NotFound();
            }
        }

        [HttpPost]
        [Route("orders/update-status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, OrderStatus status)
        {
            try
            {
                var success = await _orderService.UpdateOrderStatusAsync(id, status);

                if (success)
                {
                    return Json(new { success = true, message = "Order status updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Order not found or unable to update status." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for order {OrderId}", id);
                return Json(new { success = false, message = "An error occurred while updating the order status." });
            }
        }

        #endregion

        #region Users Management

        [Route("users")]
        public async Task<IActionResult> Users()
        {
            try
            {
                var users = await _userService.GetRecentUsersAsync(50);
                ViewBag.PageTitle = "Manage Users";
                ViewBag.ActiveMenu = "Users";
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users management page");
                return View(new List<User>());
            }
        }

        [Route("users/details/{id}")]
        public async Task<IActionResult> UserDetails(string id)
        {
            try
            {
                var userProfile = await _userService.GetUserProfileAsync(id);
                if (userProfile == null)
                {
                    return NotFound();
                }

                ViewBag.PageTitle = $"User: {userProfile.Name}";
                ViewBag.ActiveMenu = "Users";
                return View(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user details for ID: {UserId}", id);
                return NotFound();
            }
        }

        #endregion

        #region Reports & Analytics

        [Route("reports")]
        public async Task<IActionResult> Reports()
        {
            try
            {
                var model = new AdminDashboardViewModel();

                // Get comprehensive statistics for reports
                var (totalRevenue, totalOrders, averageOrderValue) = await _orderService.GetOrderStatisticsAsync();
                var monthlyRevenue = await _orderService.GetMonthlyRevenueAsync(12);
                var ordersByStatus = await _orderService.GetOrdersByStatusAsync();
                var userRegistrations = await _userService.GetUserRegistrationsAsync(12);

                model.TotalRevenue = totalRevenue;
                model.TotalOrders = totalOrders;
                model.MonthlyRevenue = monthlyRevenue;
                model.OrdersByStatus = ordersByStatus.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
                model.UserRegistrations = userRegistrations;

                ViewBag.PageTitle = "Reports & Analytics";
                ViewBag.ActiveMenu = "Reports";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reports page");
                return View(new AdminDashboardViewModel());
            }
        }

        [HttpGet]
        [Route("reports/export/{type}")]
        public async Task<IActionResult> ExportReport(string type)
        {
            try
            {
                // This would be implemented based on your reporting needs
                // For now, just return a placeholder
                return Json(new { success = false, message = "Export functionality not implemented yet" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report of type: {Type}", type);
                return Json(new { success = false, message = "Error exporting report" });
            }
        }

        #endregion

        #region Settings

        [Route("settings")]
        public IActionResult Settings()
        {
            ViewBag.PageTitle = "System Settings";
            ViewBag.ActiveMenu = "Settings";
            return View();
        }

        [HttpPost]
        [Route("settings")]
        [ValidateAntiForgeryToken]
        public IActionResult Settings(string action)
        {
            try
            {
                // Handle different settings actions here
                switch (action?.ToLower())
                {
                    case "clearcache":
                        // Clear cache logic
                        TempData["SuccessMessage"] = "Cache cleared successfully!";
                        break;
                    case "backup":
                        // Backup logic
                        TempData["SuccessMessage"] = "Backup created successfully!";
                        break;
                    default:
                        TempData["ErrorMessage"] = "Unknown action";
                        break;
                }

                return RedirectToAction(nameof(Settings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in settings action: {Action}", action);
                TempData["ErrorMessage"] = "An error occurred while processing the request.";
                return RedirectToAction(nameof(Settings));
            }
        }

        #endregion
    }
}