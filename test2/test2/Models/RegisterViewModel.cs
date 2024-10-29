using System.ComponentModel.DataAnnotations;

namespace test2.Models
{

        public class RegisterViewModel
        {
            [Required(ErrorMessage = "Tên đầy đủ là bắt buộc.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
            public string Username { get; set; }

            [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
            [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
            public string Password { get; set; }

            [Required(ErrorMessage = "Nhập lại mật khẩu là bắt buộc.")]
            [Compare("Password", ErrorMessage = "Mật khẩu không khớp.")]
            public string RePassword { get; set; }

            [Required(ErrorMessage = "Email là bắt buộc.")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
            public string Email { get; set; }

            public string Phone { get; set; }

            [Required(ErrorMessage = "Giới tính là bắt buộc.")]
            public bool Gender { get; set; }

            [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
            public DateTime DateOfBirth { get; set; }
        }

    }


