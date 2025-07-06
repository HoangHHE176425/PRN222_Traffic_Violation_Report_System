using System.ComponentModel.DataAnnotations;

namespace Traffic_Violation_Reporting_Management_System.DTOs
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "CCCD là bắt buộc")]
        [StringLength(12, MinimumLength = 9, ErrorMessage = "CCCD phải từ 9-12 ký tự")]
        [RegularExpression(@"^\d+$", ErrorMessage = "CCCD chỉ được chứa số")]
        [Display(Name = "Số CCCD")]
        public string Cccd { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 03, 05, 07, 08, 09 và có 10 số")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email không được quá 100 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Địa chỉ không được quá 255 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-50 ký tự")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Removed terms agreement requirement - auto set to true
        public bool AgreeTerms { get; set; } = true;
    }
} 