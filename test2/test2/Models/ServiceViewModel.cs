using test2.Data;

namespace test2.Models
{
    public class ServiceViewModel
    {
        public string SpecialtyId { get; set; } = null!;
        public string SpecialtyName { get; set; } = null!;
        public string SpecialtyImg { get; set; } = null!;
        public string ShortDescription { get; set; } = null!;
        public List<DetailSpecialty> DetailSpecialties { get; set; } = new List<DetailSpecialty>(); // Danh sách chi tiết specialty
        public List<Doctor> Doctors { get; set; } = new List<Doctor>(); // Danh sách bác sĩ
        public List<Feedback> Feedbacks { get; set; } = new List<Feedback>(); // Danh sách phản hồi
        public double AverageRating { get; set; } // Điểm trung bình đánh giá
        public int TotalReviews { get; set; } // Tổng số đánh giá
    }

}
