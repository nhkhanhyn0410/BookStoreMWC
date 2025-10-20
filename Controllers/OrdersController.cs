using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BookStoreMVC.Models.Entities;
using BookStoreMVC.Models.ViewModels;
using BookStoreMVC.Services;

namespace BookStoreMVC.Controllers
{
    [Authorize] // Yêu cầu đăng nhập cho toàn bộ controller
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            ICartService cartService,
            UserManager<User> userManager,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _cartService = cartService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                // Kiểm tra người dùng đã đăng nhập
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    TempData["InfoMessage"] = "Vui lòng đăng nhập để tiếp tục thanh toán.";
                    return RedirectToAction("Login", "Account", new { returnUrl = "/Orders/Checkout" });
                }

                var userId = _userManager.GetUserId(User)!;
                var cart = await _cartService.GetCartAsync(userId);

                if (cart.IsEmpty)
                {
                    TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Cart");
                }

                var user = await _userManager.GetUserAsync(User);

                var model = new OrderCreateViewModel
                {
                    Cart = cart,
                    ShippingFirstName = user?.Name?.Split(' ').FirstOrDefault() ?? "",
                    ShippingLastName = user?.Name?.Split(' ').LastOrDefault() ?? "",
                    ShippingPhone = user?.PhoneNumber ?? "",
                    AvailablePaymentMethods = new[] { "Credit Card", "PayPal", "Bank Transfer", "Cash on Delivery" },
                    AvailableCountries = new[] { "Vietnam", "United States", "United Kingdom", "Canada", "Australia" }
                };

                ViewBag.PageTitle = "Checkout";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading checkout page");
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(OrderCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var userId = _userManager.GetUserId(User)!;
                    model.Cart = await _cartService.GetCartAsync(userId);
                    model.AvailablePaymentMethods = new[] { "Credit Card", "PayPal", "Bank Transfer", "Cash on Delivery" };
                    model.AvailableCountries = new[] { "Vietnam", "United States", "United Kingdom", "Canada", "Australia" };
                    return View(model);
                }

                var currentUserId = _userManager.GetUserId(User)!;
                var order = await _orderService.CreateOrderAsync(currentUserId, model);

                if (order != null)
                {
                    TempData["SuccessMessage"] = $"Đơn hàng {order.OrderNumber} đã được tạo thành công!";
                    return RedirectToAction(nameof(Details), new { id = order.Id });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không thể tạo đơn hàng. Vui lòng thử lại.");
                    model.Cart = await _cartService.GetCartAsync(currentUserId);
                    return View(model);
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var userId = _userManager.GetUserId(User)!;
                model.Cart = await _cartService.GetCartAsync(userId);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi tạo đơn hàng.");
                var userId = _userManager.GetUserId(User)!;
                model.Cart = await _cartService.GetCartAsync(userId);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var orders = await _orderService.GetUserOrdersAsync(userId);

                ViewBag.PageTitle = "Đơn hàng của tôi";
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders");
                return View(new List<OrderViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null || order.UserId != userId)
                {
                    return NotFound();
                }

                ViewBag.PageTitle = $"Order {order.OrderNumber}";
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details");
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var success = await _orderService.CancelOrderAsync(id, userId);

                if (success)
                {
                    TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể hủy đơn hàng này.";
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi hủy đơn hàng.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}