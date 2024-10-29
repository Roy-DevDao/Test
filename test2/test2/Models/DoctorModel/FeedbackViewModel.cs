namespace test2.Models.DoctorModel
{
    public class FeedbackViewModel
    {
        public string FeedbackId { get; set; }
        public string? PatientName { get; set; }
        public DateTime? DateCmt { get; set; }
        public int? Star { get; set; }
        public string? Description { get; set; }
        public BaseViewModel basevm { get; set; } = new BaseViewModel();

    }
}