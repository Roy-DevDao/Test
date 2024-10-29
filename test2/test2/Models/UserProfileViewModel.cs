using System.ComponentModel.DataAnnotations;

namespace test2.Models
{
    public class UserProfileViewModel
    {
        public string Id { get; set; } = null!;

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? Email { get; set; }

        public int? Role { get; set; }

        public bool? Status { get; set; }

        // Thông tin dành riêng cho bệnh nhân (role = 3)
        public string? PatientName { get; set; }

        public string? PatientImg { get; set; }

        public string? PPhone { get; set; }

        public string? PGender { get; set; }

        public DateOnly? PDob { get; set; }

        // Thông tin dành riêng cho bác sĩ (role = 2)
        public string? DoctorName { get; set; }

        public string? DoctorImg { get; set; }

        public string? Position { get; set; }

        public string? DPhone { get; set; }

        public string? DGender { get; set; }

        public DateOnly? DDob { get; set; }

        public string? Description { get; set; }

        public double? Price { get; set; }

        public string? SpecialtyId { get; set; }

        // Thông tin dành riêng cho staff
        public string? StaffName { get; set; }

        public string? StaffImg { get; set; }

        public string? SPhone { get; set; }

        public string? SGender { get; set; }

        public DateOnly? SDob { get; set; }

		// Thuộc tính cho việc đổi mật khẩu
		[Required(ErrorMessage = "Mật khẩu cũ không được để trống.")]
		[DataType(DataType.Password)]
		public string? OldPassword { get; set; }

		[Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
		[DataType(DataType.Password)]
		[StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải từ 6 đến 100 ký tự.")]
		public string? NewPassword { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới.")]
		[DataType(DataType.Password)]
		[Compare("NewPassword", ErrorMessage = "Mật khẩu mới và nhập lại không khớp.")]
		public string? RePassword { get; set; }
	}

}
