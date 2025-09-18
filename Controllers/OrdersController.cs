// Controllers/OrdersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BookStoreMVC.Models.Entities;
using BookStoreMVC.Models.ViewModels;
using BookStoreMVC.Services;

namespace BookStoreMVC.Controllers
{
    [Authorize]
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

        public async Task<IActionResult> Index(OrderListViewModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var (orders, totalCount) = await _orderService.GetOrdersAsync(model, userId);

                model.Orders = orders;
                model.TotalCount = totalCount;

                ViewBag.PageTitle = "My Orders";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user orders");
                return View(new OrderListViewModel());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var order = await _orderService.GetOrderByIdAsync(id, userId);

                if (order == null)
                {
                    return NotFound();
                }

                ViewBag.PageTitle = $"Order {order.OrderNumber}";
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details for ID: {OrderId}", id);
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var cart = await _cartService.GetCartAsync(userId);

                if (cart.IsEmpty)
                {
                    TempData["ErrorMessage"] = "Your cart is empty.";
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
                    TempData["SuccessMessage"] = $"Order {order.OrderNumber} has been created successfully!";
                    return RedirectToAction(nameof(Details), new { id = order.Id });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Unable to create order. Please try again.");
                    model.Cart = await _cartService.GetCartAsync(currentUserId);
                    return View(model);
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var userId = _userManager.GetUserId(User)!;
                model.Cart = await _cartService.GetCartAsync(userId);
                model.AvailablePaymentMethods = new[] { "Credit Card", "PayPal", "Bank Transfer", "Cash on Delivery" };
                model.AvailableCountries = new[] { "Vietnam", "United States", "United Kingdom", "Canada", "Australia" };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                ModelState.AddModelError(string.Empty, "An error occurred while processing your order.");
                var userId = _userManager.GetUserId(User)!;
                model.Cart = await _cartService.GetCartAsync(userId);
                return View(model);
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
                    TempData["SuccessMessage"] = "Order has been cancelled successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to cancel order. Please contact customer service.";
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                TempData["ErrorMessage"] = "An error occurred while cancelling the order.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}