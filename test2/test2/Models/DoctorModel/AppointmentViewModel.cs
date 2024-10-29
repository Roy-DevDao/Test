namespace test2.Models.DoctorModel
{
    public class AppointmentViewModel
    {
        public string AppointmentId { get; set; }
        public string? PatientName { get; set; }
        public string? PatientImage { get; set; }
        public DateTime? DateOrder { get; set; }  // Ngày hẹn
        public string? Status { get; set; }

        // Thêm thông tin về bác sĩ

        public BaseViewModel basevm { get; set; } = new BaseViewModel();
    }
}
