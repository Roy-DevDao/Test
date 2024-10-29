namespace test2.Models.DoctorModel
{
    public class DoctorProfileViewModel
    {
        // ID của bác sĩ, không được phép null

        // Thông tin tài khoản bác sĩ
        public string? Username { get; set; }
        public string? Email { get; set; }
        public int? Role { get; set; }
        public bool? Status { get; set; }

        // Thông tin cá nhân của bác sĩ
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public DateOnly? Dob { get; set; } // Giữ nguyên kiểu này

        // Vị trí công việc của bác sĩ
        public string? Position { get; set; }

        // Thông tin về chuyên khoa
        public string? Specialty { get; set; }

        // Mô tả về bác sĩ
        public string? Description { get; set; }

        // Giá dịch vụ khám bệnh của bác sĩ
        public double? Price { get; set; } // Sử dụng double? nếu giá có thể null

        // Hình ảnh của bác sĩ
        public BaseViewModel basevm { get; set; } = new BaseViewModel();
    }

}
