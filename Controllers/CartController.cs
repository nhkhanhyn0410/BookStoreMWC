using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BookStoreMVC.Models.Entities;
using BookStoreMVC.Services;

namespace BookStoreMVC.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IBookService _bookService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ICartService cartService,
            IBookService bookService,
            UserManager<User> userManager,
            ILogger<CartController> logger)
        {
            _cartService = cartService;
            _bookService = bookService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var cart = await _cartService.GetCartAsync(userId);

                return View(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart");
                return View(new Models.ViewModels.CartViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId, int quantity = 1)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var success = await _cartService.AddToCartAsync(userId, bookId, quantity);

                if (success)
                {
                    var itemCount = await _cartService.GetCartItemCountAsync(userId);
                    var total = await _cartService.GetCartTotalAsync(userId);

                    return Json(new
                    {
                        success = true,
                        message = "Item added to cart successfully",
                        cartItemCount = itemCount,
                        cartTotal = total
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Failed to add item to cart. Please check stock availability."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding book {BookId} to cart", bookId);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while adding the item to cart."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(int bookId, int quantity)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var success = await _cartService.UpdateCartItemAsync(userId, bookId, quantity);

                if (success)
                {
                    var cart = await _cartService.GetCartAsync(userId);
                    var itemCount = await _cartService.GetCartItemCountAsync(userId);

                    return Json(new
                    {
                        success = true,
                        message = "Cart updated successfully",
                        cartItemCount = itemCount,
                        subtotal = cart.SubTotal,
                        shippingCost = cart.ShippingCost,
                        tax = cart.Tax,
                        total = cart.Total
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Failed to update cart item. Please check stock availability."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item for book {BookId}", bookId);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while updating the cart."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int bookId)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var success = await _cartService.RemoveFromCartAsync(userId, bookId);

                if (success)
                {
                    var cart = await _cartService.GetCartAsync(userId);
                    var itemCount = await _cartService.GetCartItemCountAsync(userId);

                    return Json(new
                    {
                        success = true,
                        message = "Item removed from cart",
                        cartItemCount = itemCount,
                        subtotal = cart.SubTotal,
                        shippingCost = cart.ShippingCost,
                        tax = cart.Tax,
                        total = cart.Total,
                        isEmpty = cart.IsEmpty
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Failed to remove item from cart."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing book {BookId} from cart", bookId);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while removing the item from cart."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var success = await _cartService.ClearCartAsync(userId);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Cart cleared successfully",
                        cartItemCount = 0
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Failed to clear cart."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return Json(new
                {
                    success = false,
                    message = "An error occurred while clearing the cart."
                });
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

        [HttpGet]
        public async Task<IActionResult> GetCartSummary()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false });
                }

                var cart = await _cartService.GetCartAsync(userId);

                var summary = new
                {
                    success = true,
                    itemCount = cart.TotalItems,
                    subtotal = cart.SubTotal,
                    shippingCost = cart.ShippingCost,
                    tax = cart.Tax,
                    total = cart.Total,
                    items = cart.Items.Select(item => new
                    {
                        bookId = item.BookId,
                        title = item.Book.Title,
                        quantity = item.Quantity,
                        price = item.Book.FinalPrice,
                        total = item.TotalPrice,
                        imageUrl = item.Book.ImageUrl
                    })
                };

                return Json(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart summary");
                return Json(new { success = false });
            }
        }

        // Quick add to cart with redirect
        [HttpPost]
        public async Task<IActionResult> QuickAdd(int bookId, string returnUrl)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var success = await _cartService.AddToCartAsync(userId, bookId, 1);

                if (success)
                {
                    TempData["SuccessMessage"] = "Item added to cart successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to add item to cart. Please check stock availability.";
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Books");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in quick add for book {BookId}", bookId);
                TempData["ErrorMessage"] = "An error occurred while adding the item to cart.";
                return RedirectToAction("Index", "Books");
            }
        }
    }
}