using System.ComponentModel.DataAnnotations;

namespace test2.Models.PatientModel
{
    public class UpdateProfileViewModel
    {
        public string PId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public int? Role { get; set; }
        public bool? Status { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên.")]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự.")]
        //[RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public DateOnly? Dob { get; set; }
        public string? PatientImg { get; set; }
        public IFormFile? AvataUpload { get; set; }

        // Thuộc tính string để tương thích với view
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public string DobString
        {
            get => Dob?.ToString("yyyy-MM-dd");
            set => Dob = DateOnly.Parse(value);
        }
        // Hàm xác minh ngày sinh
        public static ValidationResult? ValidateDob(DateOnly? dob, ValidationContext context)
        {
            if (dob.HasValue && dob.Value > DateOnly.FromDateTime(DateTime.Now))
            {
                return new ValidationResult("Ngày sinh phải trước ngày hiện tại.");
            }
            return ValidationResult.Success;
        }
    }
}