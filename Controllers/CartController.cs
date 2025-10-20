using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BookStoreMVC.Models.Entities;
using BookStoreMVC.Models.ViewModels;
using BookStoreMVC.Services;

namespace BookStoreMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ISessionCartService _sessionCartService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ICartService cartService,
            ISessionCartService sessionCartService,
            UserManager<User> userManager,
            ILogger<CartController> logger)
        {
            _cartService = cartService;
            _sessionCartService = sessionCartService;
            _userManager = userManager;
            _logger = logger;
        }

        // Cho phép xem giỏ hàng không cần đăng nhập
        public async Task<IActionResult> Index()
        {
            try
            {
                CartViewModel cart;

                if (User.Identity?.IsAuthenticated == true)
                {
                    // Người dùng đã đăng nhập - load từ database
                    var userId = _userManager.GetUserId(User)!;
                    cart = await _cartService.GetCartAsync(userId);
                }
                else
                {
                    // Khách - load từ session
                    cart = _sessionCartService.GetCart();
                }

                ViewBag.PageTitle = "Shopping Cart";
                return View(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart");
                return View(new CartViewModel());
            }
        }

        // Cho phép thêm vào giỏ hàng không cần đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(AddToCartViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid request" });
                }

                bool success;
                int itemCount;

                if (User.Identity?.IsAuthenticated == true)
                {
                    // Người dùng đã đăng nhập
                    var userId = _userManager.GetUserId(User)!;
                    success = await _cartService.AddToCartAsync(userId, model);
                    itemCount = await _cartService.GetCartItemCountAsync(userId);
                }
                else
                {
                    // Khách - lưu vào session
                    _sessionCartService.AddToCart(model.BookId, model.Quantity);
                    success = true;
                    itemCount = _sessionCartService.GetCartItemCount();
                }

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Đã thêm vào giỏ hàng!",
                        cartItemCount = itemCount
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể thêm sản phẩm. Vui lòng kiểm tra tồn kho."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi thêm sản phẩm vào giỏ hàng."
                });
            }
        }

        // Cho phép cập nhật số lượng không cần đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartItem(UpdateCartItemViewModel model)
        {
            try
            {
                bool success;
                CartViewModel cart;

                if (User.Identity?.IsAuthenticated == true)
                {
                    // Người dùng đã đăng nhập
                    var userId = _userManager.GetUserId(User)!;
                    success = await _cartService.UpdateCartItemAsync(userId, model.BookId, model.Quantity);
                    cart = await _cartService.GetCartAsync(userId);
                }
                else
                {
                    // Khách - cập nhật session
                    _sessionCartService.UpdateCartItem(model.BookId, model.Quantity);
                    cart = _sessionCartService.GetCart();
                    success = true;
                }

                if (success)
                {
                    // Tìm item total cho sản phẩm vừa cập nhật
                    var item = cart.Items.FirstOrDefault(i => i.BookId == model.BookId);
                    var itemTotal = item != null ? (item.Price * item.Quantity).ToString("C0") : "0 ₫";

                    return Json(new
                    {
                        success = true,
                        message = "Đã cập nhật giỏ hàng!",
                        itemTotal = itemTotal,
                        cart = new
                        {
                            subTotal = cart.SubTotal.ToString("C0"),
                            tax = cart.Tax.ToString("C0"),
                            shippingCost = cart.ShippingCost.ToString("C0"),
                            total = cart.Total.ToString("C0"),
                            itemCount = cart.ItemCount
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể cập nhật số lượng." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart quantity");
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi cập nhật giỏ hàng."
                });
            }
        }

        // Cho phép xóa sản phẩm không cần đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(RemoveFromCartViewModel model)
        {
            try
            {
                bool success;
                CartViewModel cart;

                if (User.Identity?.IsAuthenticated == true)
                {
                    // Người dùng đã đăng nhập
                    var userId = _userManager.GetUserId(User)!;
                    success = await _cartService.RemoveFromCartAsync(userId, model.BookId);
                    cart = await _cartService.GetCartAsync(userId);
                }
                else
                {
                    // Khách - xóa khỏi session
                    _sessionCartService.RemoveFromCart(model.BookId);
                    cart = _sessionCartService.GetCart();
                    success = true;
                }

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Đã xóa khỏi giỏ hàng!",
                        cart = new
                        {
                            subTotal = cart.SubTotal.ToString("C0"),
                            tax = cart.Tax.ToString("C0"),
                            shippingCost = cart.ShippingCost.ToString("C0"),
                            total = cart.Total.ToString("C0"),
                            itemCount = cart.ItemCount
                        }
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể xóa sản phẩm khỏi giỏ hàng."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart");
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi xóa sản phẩm."
                });
            }
        }

        // Cho phép xóa toàn bộ giỏ hàng không cần đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                bool success;

                if (User.Identity?.IsAuthenticated == true)
                {
                    // Người dùng đã đăng nhập
                    var userId = _userManager.GetUserId(User)!;
                    success = await _cartService.ClearCartAsync(userId);
                }
                else
                {
                    // Khách - xóa session
                    _sessionCartService.ClearCart();
                    success = true;
                }

                if (success)
                {
                    return Json(new { success = true, message = "Đã xóa toàn bộ giỏ hàng!" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể xóa giỏ hàng." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi xóa giỏ hàng."
                });
            }
        }

        // Lấy số lượng items trong giỏ hàng
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                int count;

                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = _userManager.GetUserId(User);
                    count = await _cartService.GetCartItemCountAsync(userId!);
                }
                else
                {
                    count = _sessionCartService.GetCartItemCount();
                }

                return Json(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart count");
                return Json(new { count = 0 });
            }
        }

        // Action để migrate giỏ hàng từ session sang database khi user đăng nhập
        // Gọi action này trong AccountController sau khi login thành công
        [Authorize]
        public async Task<IActionResult> MigrateGuestCart()
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var sessionCart = _sessionCartService.GetCart();

                if (sessionCart.Items.Any())
                {
                    // Chuyển từng item từ session cart sang database cart
                    foreach (var item in sessionCart.Items)
                    {
                        await _cartService.AddToCartAsync(userId, new AddToCartViewModel
                        {
                            BookId = item.BookId,
                            Quantity = item.Quantity
                        });
                    }

                    // Xóa session cart sau khi migrate
                    _sessionCartService.ClearCart();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error migrating guest cart");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}