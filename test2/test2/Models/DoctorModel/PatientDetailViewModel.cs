namespace test2.Models.DoctorModel
{
    public class PatientDetailViewModel
    {
        public string Pid { get; set; } = null!;
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateOnly? Dob { get; set; }
        public List<AppointmentViewModel> Appointments { get; set; } = new List<AppointmentViewModel>();

        public class AppointmentViewModel
        {
            public string? Date { get; set; }
            public string? Time { get; set; }
            public string? Status { get; set; }
        }
        public BaseViewModel basevm { get; set; } = new BaseViewModel();

    }
}