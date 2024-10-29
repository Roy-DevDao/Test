using System.ComponentModel.DataAnnotations;

namespace test2.Models.AdminModel
{
    public class AddSpecialtyViewModel
    {
        [Required]
        public string SpecialtyId { get; set; }

        [Required]
        public string SpecialtyName { get; set; }

        public string? SpecialtyImg { get; set; }

        public string? ShortDescription { get; set; }

        [Display(Name = "Ảnh Dịch Vụ")]
        public IFormFile? SpecialtyImgUpload { get; set; } // File for image upload
    }
}
