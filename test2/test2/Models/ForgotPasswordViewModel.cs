using System.ComponentModel.DataAnnotations;

namespace test2.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        // Thông báo lỗi hoặc thành công (nếu có)
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
    }
}