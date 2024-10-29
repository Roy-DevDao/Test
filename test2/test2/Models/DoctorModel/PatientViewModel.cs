namespace test2.Models.DoctorModel
{
    public class PatientViewModel
    {
        public string Pid { get; set; } = null!;
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOrder { get; set; }
        public DateOnly? Dob { get; set; }
        public BaseViewModel basevm { get; set; } = new BaseViewModel();

    }
}