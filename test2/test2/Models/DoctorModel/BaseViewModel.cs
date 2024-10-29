using System.ComponentModel.DataAnnotations;
using test2.Models.StaffModel;

namespace test2.Models.DoctorModel
{
    public class BaseViewModel
    {
        public string? DoctorImg { get; set; }
        public string DId { get; set; } = null!;
        public string? Name { get; set; }

        public DoctorProfileViewModel doctorProfile { get; set; }

        public AppointmentViewModel appointmentlist { get; set; }

        public AppointmentDetailViewModel appointmentDetail { get; set; }

        public PatientViewModel patientView { get; set; }

        public PatientDetailViewModel patientDetail { get; set; }

        public FeedbackViewModel feedbackView { get; set; }

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