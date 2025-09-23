using System.ComponentModel.DataAnnotations;

namespace BookStoreMVC.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ đăng nhập?")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng đồng ý với điều khoản sử dụng")]
        [Display(Name = "Tôi đồng ý với điều khoản sử dụng")]
        public bool AgreeToTerms { get; set; }

        [Display(Name = "Nhận email khuyến mãi")]
        public bool ReceivePromotions { get; set; } = true;

        public string? ReturnUrl { get; set; }
    }

    public class UserProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        // Thống kê hồ sơ
        [Display(Name = "Tổng số đơn hàng")]
        public int TotalOrders { get; set; }

        [Display(Name = "Tổng chi tiêu")]
        public decimal TotalSpent { get; set; }

        [Display(Name = "Số lượng đánh giá")]
        public int ReviewsCount { get; set; }

        [Display(Name = "Danh sách yêu thích")]
        public int WishlistCount { get; set; }

        [Display(Name = "Thành viên từ")]
        public DateTime MemberSince { get; set; }

        // Hoạt động gần đây
        public IEnumerable<OrderViewModel> RecentOrders { get; set; } = new List<OrderViewModel>();
        public IEnumerable<ReviewViewModel> RecentReviews { get; set; } = new List<ReviewViewModel>();
    }

    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email đã đăng ký")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email đã đăng ký")]
        public string Email { get; set; } = string.Empty;
    }

    // public class ResetPasswordViewModel
    // {
    //     [Required(ErrorMessage = "Vui lòng nhập email đã đăng ký")]
    //     [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    //     [Display(Name = "Email")]
    //     public string Email { get; set; } = string.Empty;

    //     [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
    //     [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự")]
    //     [DataType(DataType.Password)]
    //     [Display(Name = "Mật khẩu mới")]
    //     public string NewPassword { get; set; } = string.Empty;

    //     [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
    //     [DataType(DataType.Password)]
    //     [Display(Name = "Xác nhận mật khẩu mới")]
    //     [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    //     public string ConfirmPassword { get; set; } = string.Empty;

    //     public string Token { get; set; } = string.Empty;
    // }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có ít nhất {2} ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
