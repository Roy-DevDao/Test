using System.ComponentModel.DataAnnotations;

namespace test2.Models.AdminModel
{
    public class AddDoctorViewModel
    {
        [Required]
        public string Did { get; set; } = null!;

        [Required]
        public string Name { get; set; }

        public string? Position { get; set; }

        public string? Phone { get; set; }

        public string? Gender { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
        [DataType(DataType.Date)]
        [CheckAge(20, ErrorMessage = "Bác sĩ phải trên 20 tuổi.")]
        public DateOnly? Dob { get; set; }
        public string? Description { get; set; }

        public double? Price { get; set; }

        public string? SpecialtyId { get; set; }

        public string? DoctorImg { get; set; }

        [Display(Name = "Ảnh Bác Sĩ")]
        public IFormFile? DoctorImgUpload { get; set; } // File for image upload
    }
    public class CheckAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public CheckAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateOnly dob)
            {
                // Convert DateOnly to DateTime for comparison
                DateTime dobDateTime = dob.ToDateTime(TimeOnly.MinValue);
                int age = DateTime.Now.Year - dobDateTime.Year;

                if (dobDateTime.AddYears(age) >= DateTime.Now)
                    age--;

                if (age < _minimumAge)
                    return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
