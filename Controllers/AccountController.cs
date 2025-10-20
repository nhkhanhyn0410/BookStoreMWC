using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BookStoreMVC.Models.Entities;
using BookStoreMVC.Models.ViewModels;
using BookStoreMVC.Services;

namespace BookStoreMVC.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ICartService _cartService;
        private readonly ISessionCartService _sessionCartService; // ← THÊM DÒNG NÀY
        private readonly ILogger<AccountController> _logger;

        // ===================================================================
        // CONSTRUCTOR - Thêm ISessionCartService và ICartService
        // ===================================================================
        public AccountController(
            IUserService userService,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ICartService cartService,              // ← THÊM
            ISessionCartService sessionCartService, // ← THÊM
            ILogger<AccountController> logger)
        {
            _userService = userService;
            _userManager = userManager;
            _signInManager = signInManager;
            _cartService = cartService;              // ← THÊM
            _sessionCartService = sessionCartService; // ← THÊM
            _logger = logger;
        }

        // ===================================================================
        // DASHBOARD
        // ===================================================================
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var dashboard = await _userService.GetUserDashboardAsync(userId);

                ViewBag.PageTitle = "My Account";
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user dashboard");
                return View(new UserDashboardViewModel());
            }
        }

        // ===================================================================
        // PROFILE
        // ===================================================================
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var profile = await _userService.GetUserProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound();
                }

                ViewBag.PageTitle = "My Profile";
                return View(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user profile");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userId = _userManager.GetUserId(User)!;
                var success = await _userService.UpdateUserProfileAsync(userId, model);

                if (!success)
                {
                    ModelState.AddModelError("", "Không thể cập nhật hồ sơ.");
                    return View(model);
                }

                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật hồ sơ.");
                return View(model);
            }
        }

        // ===================================================================
        // LOGIN - VỚI CART MIGRATION
        // ===================================================================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.PageTitle = "Đăng nhập";
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Lưu thông tin giỏ hàng session TRƯỚC KHI đăng nhập
                var sessionCartItems = _sessionCartService.GetCart().Items.ToList();

                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true
                );

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    // ===================================================================
                    // MIGRATE GIỎ HÀNG TỪ SESSION SANG DATABASE
                    // ===================================================================
                    if (sessionCartItems.Any())
                    {
                        var userId = _userManager.GetUserId(User)!;

                        foreach (var item in sessionCartItems)
                        {
                            try
                            {
                                await _cartService.AddToCartAsync(userId, new AddToCartViewModel
                                {
                                    BookId = item.BookId,
                                    Quantity = item.Quantity
                                });
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error migrating cart item {item.BookId}");
                            }
                        }

                        // Xóa session cart sau khi migrate thành công
                        _sessionCartService.ClearCart();

                        TempData["SuccessMessage"] = "Đăng nhập thành công! Giỏ hàng của bạn đã được cập nhật.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Đăng nhập thành công!";
                    }

                    // Redirect
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa do đăng nhập sai quá nhiều lần. Vui lòng thử lại sau.");
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi trong quá trình đăng nhập. Vui lòng thử lại.");
                return View(model);
            }
        }

        // ===================================================================
        // REGISTER - VỚI CART MIGRATION
        // ===================================================================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.PageTitle = "Đăng ký";
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            try
            {
                ViewData["ReturnUrl"] = returnUrl;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Lưu giỏ hàng session trước khi đăng ký
                var sessionCartItems = _sessionCartService.GetCart().Items.ToList();

                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.FullName,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Add to Customer role
                    await _userManager.AddToRoleAsync(user, "Customer");

                    // Sign in user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // ===================================================================
                    // MIGRATE GIỎ HÀNG SAU KHI ĐĂNG KÝ
                    // ===================================================================
                    if (sessionCartItems.Any())
                    {
                        foreach (var item in sessionCartItems)
                        {
                            try
                            {
                                await _cartService.AddToCartAsync(user.Id, new AddToCartViewModel
                                {
                                    BookId = item.BookId,
                                    Quantity = item.Quantity
                                });
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error migrating cart item {item.BookId} after registration");
                            }
                        }

                        // Xóa session cart
                        _sessionCartService.ClearCart();

                        TempData["SuccessMessage"] = "Đăng ký thành công! Giỏ hàng của bạn đã được lưu.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Đăng ký thành công! Chào mừng đến với BookStore!";
                    }

                    return LocalRedirect(returnUrl ?? "/");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi trong quá trình đăng ký.");
                return View(model);
            }
        }

        // ===================================================================
        // LOGOUT
        // ===================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            TempData["InfoMessage"] = "Bạn đã đăng xuất thành công.";
            return RedirectToAction("Index", "Home");
        }

        // ===================================================================
        // FORGOT PASSWORD
        // ===================================================================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            ViewBag.PageTitle = "Quên mật khẩu";
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Không tiết lộ là email này chưa đăng ký (bảo mật)
                TempData["InfoMessage"] = "Nếu email này tồn tại trong hệ thống, một liên kết đặt lại mật khẩu đã được gửi.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account",
                new { userId = user.Id, token = token }, protocol: Request.Scheme);

            // TODO: Gửi email callbackUrl cho user
            // await _emailService.SendPasswordResetEmailAsync(model.Email, callbackUrl);

            TempData["SuccessMessage"] = "Liên kết đặt lại mật khẩu đã được gửi đến email của bạn.";
            return RedirectToAction(nameof(Login));
        }

        // ===================================================================
        // RESET PASSWORD
        // ===================================================================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string? userId = null, string? token = null)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                UserId = userId
            };

            ViewBag.PageTitle = "Đặt lại mật khẩu";
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Mật khẩu đã được đặt lại thành công. Vui lòng đăng nhập.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // ===================================================================
        // TWO FACTOR AUTHENTICATION (Optional)
        // ===================================================================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string? returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.PageTitle = "Xác thực 2 yếu tố";

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
                authenticatorCode,
                model.RememberMe,
                model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                return LocalRedirect(returnUrl ?? "/");
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                ModelState.AddModelError(string.Empty, "Tài khoản bị khóa.");
                return View(model);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Mã xác thực không hợp lệ.");
                return View(model);
            }
        }

        // ===================================================================
        // ACCESS DENIED
        // ===================================================================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            ViewBag.PageTitle = "Truy cập bị từ chối";
            return View();
        }
    }
}