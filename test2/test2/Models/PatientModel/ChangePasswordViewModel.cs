using System.ComponentModel.DataAnnotations;

namespace test2.Models.PatientModel
{
    public class ChangePasswordViewModel
    {
        public string PId { get; set; }
        // Thuộc tính cho đổi mật khẩu
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [MinLength(8, ErrorMessage = "Mật khẩu mới phải có ít nhất 8 ký tự.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới.")]
        [MinLength(8, ErrorMessage = "Xác nhận mật khẩu mới phải có ít nhất 8 ký tự.")]
        public string ConfirmNewPassword { get; set; }
    }
}