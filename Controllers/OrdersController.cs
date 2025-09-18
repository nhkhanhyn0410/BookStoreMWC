using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var orders = await _orderService.GetOrdersByUserAsync(userId);

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user orders");
                return View(new List<Order>());
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
                    ShippingFirstName = user?.FirstName ?? "",
                    ShippingLastName = user?.LastName ?? "",
                    ShippingAddress = user?.Address ?? "",
                    ShippingCity = user?.City ?? "",
                    ShippingPostalCode = user?.PostalCode ?? "",
                    ShippingCountry = user?.Country ?? "",
                    ShippingPhone = user?.PhoneNumber ?? ""
                };

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
                var userId = _userManager.GetUserId(User)!;

                // Reload cart data
                model.Cart = await _cartService.GetCartAsync(userId);

                if (model.Cart.IsEmpty)
                {
                    TempData["ErrorMessage"] = "Your cart is empty.";
                    return RedirectToAction("Index", "Cart");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var order = await _orderService.CreateOrderAsync(userId, model);

                if (order != null)
                {
                    _logger.LogInformation("Order {OrderNumber} created successfully for user {UserId}",
                        order.OrderNumber, userId);

                    return RedirectToAction(nameof(Confirmation), new { id = order.Id });
                }

                ModelState.AddModelError(string.Empty, "Failed to create order. Please try again.");
                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during checkout");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout process");
                ModelState.AddModelError(string.Empty, "An error occurred while processing your order. Please try again.");
                return View(model);
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

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details for order {OrderId}", id);
                return NotFound();
            }
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var order = await _orderService.GetOrderByIdAsync(id, userId);

                if (order == null)
                {
                    return NotFound();
                }

                // Mark this as a fresh confirmation page view
                ViewBag.IsNewOrder = TempData["IsNewOrder"] ?? false;

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order confirmation for order {OrderId}", id);
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var success = await _orderService.CancelOrderAsync(id, userId);

                if (success)
                {
                    TempData["SuccessMessage"] = "Order cancelled successfully.";
                    return Json(new { success = true, message = "Order cancelled successfully" });
                }

                return Json(new { success = false, message = "Unable to cancel order. Only pending orders can be cancelled." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                return Json(new { success = false, message = "An error occurred while cancelling the order." });
            }
        }

        // AJAX endpoints
        [HttpGet]
        public async Task<IActionResult> GetOrderStatus(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var order = await _orderService.GetOrderByIdAsync(id, userId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }

                return Json(new
                {
                    success = true,
                    status = order.Status.ToString(),
                    statusDisplay = order.StatusDisplay,
                    statusColor = order.StatusColor,
                    paymentStatus = order.PaymentStatus.ToString(),
                    trackingNumber = order.TrackingNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order status for order {OrderId}", id);
                return Json(new { success = false, message = "Error retrieving order status" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateShippingAddress(int orderId, OrderCreateViewModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var order = await _orderService.GetOrderByIdAsync(orderId, userId);

                if (order == null || order.Status != OrderStatus.Pending)
                {
                    return Json(new { success = false, message = "Order cannot be modified" });
                }

                // Update shipping address logic would go here
                // This is a simplified example

                return Json(new { success = true, message = "Shipping address updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shipping address for order {OrderId}", orderId);
                return Json(new { success = false, message = "Error updating shipping address" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TrackOrder(string orderNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(orderNumber))
                {
                    return Json(new { success = false, message = "Order number is required" });
                }

                // In a real application, this would integrate with shipping provider APIs
                // For demo purposes, we'll return mock tracking data

                var trackingInfo = new
                {
                    success = true,
                    orderNumber,
                    currentStatus = "In Transit",
                    estimatedDelivery = DateTime.Now.AddDays(2).ToString("MMM dd, yyyy"),
                    trackingEvents = new[]
                    {
                        new { date = DateTime.Now.AddDays(-2).ToString("MMM dd, yyyy"), status = "Order Confirmed", location = "Warehouse" },
                        new { date = DateTime.Now.AddDays(-1).ToString("MMM dd, yyyy"), status = "Shipped", location = "Distribution Center" },
                        new { date = DateTime.Now.ToString("MMM dd, yyyy"), status = "In Transit", location = "Local Facility" }
                    }
                };

                return Json(trackingInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking order {OrderNumber}", orderNumber);
                return Json(new { success = false, message = "Error retrieving tracking information" });
            }
        }
    }
}