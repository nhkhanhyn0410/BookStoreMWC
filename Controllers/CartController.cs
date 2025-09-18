using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BookStoreMVC.Models.Entities;
using BookStoreMVC.Models.ViewModels;
using BookStoreMVC.Services;

namespace BookStoreMVC.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ICartService cartService,
            UserManager<User> userManager,
            ILogger<CartController> logger)
        {
            _cartService = cartService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var cart = await _cartService.GetCartAsync(userId);

                ViewBag.PageTitle = "Shopping Cart";
                return View(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart");
                return View(new CartViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(AddToCartViewModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;

                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid request" });
                }

                var success = await _cartService.AddToCartAsync(userId, model);

                if (success)
                {
                    var itemCount = await _cartService.GetCartItemCountAsync(userId);
                    return Json(new { success = true, message = "Item added to cart!", cartItemCount = itemCount });
                }
                else
                {
                    return Json(new { success = false, message = "Unable to add item to cart. Please check stock availability." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return Json(new { success = false, message = "An error occurred while adding the item to cart." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int bookId, int quantity)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var success = await _cartService.UpdateCartItemAsync(userId, bookId, quantity);

                if (success)
                {
                    var cart = await _cartService.GetCartAsync(userId);
                    return Json(new
                    {
                        success = true,
                        message = "Cart updated!",
                        subtotal = cart.SubTotal,
                        tax = cart.Tax,
                        shippingCost = cart.ShippingCost,
                        total = cart.Total,
                        itemCount = cart.ItemCount
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Unable to update quantity." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart quantity");
                return Json(new { success = false, message = "An error occurred while updating the cart." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int bookId)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var success = await _cartService.RemoveFromCartAsync(userId, bookId);

                if (success)
                {
                    var cart = await _cartService.GetCartAsync(userId);
                    return Json(new
                    {
                        success = true,
                        message = "Item removed from cart!",
                        subtotal = cart.SubTotal,
                        tax = cart.Tax,
                        shippingCost = cart.ShippingCost,
                        total = cart.Total,
                        itemCount = cart.ItemCount,
                        isEmpty = cart.IsEmpty
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Unable to remove item from cart." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart");
                return Json(new { success = false, message = "An error occurred while removing the item." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var success = await _cartService.ClearCartAsync(userId);

                if (success)
                {
                    return Json(new { success = true, message = "Cart cleared successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Unable to clear cart." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return Json(new { success = false, message = "An error occurred while clearing the cart." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { count = 0 });
                }

                var count = await _cartService.GetCartItemCountAsync(userId);
                return Json(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart count");
                return Json(new { count = 0 });
            }
        }
    }
}