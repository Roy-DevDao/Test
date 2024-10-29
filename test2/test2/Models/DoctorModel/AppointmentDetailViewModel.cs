namespace test2.Models.DoctorModel
{
    public class AppointmentDetailViewModel
    {
        public string AppointmentId { get; set; }
        public string? PatientName { get; set; }
        public string? PatientImage { get; set; }
        public string? PatientPhone { get; set; }
        public string? Specialty { get; set; }
        public double? Price { get; set; }
        public DateTime? DateOrder { get; set; }
        public string? Symptom { get; set; }
        public string? Status { get; set; }

        public BaseViewModel basevm { get; set; } = new BaseViewModel();
    }
}