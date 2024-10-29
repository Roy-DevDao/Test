using test2.Data;

namespace test2.Models.PatientModel
{
    public class DoctorScheduleViewModel
    {
        public Doctor Doctor { get; set; }
        public List<Option> Schedule { get; set; }
        public DateTime Today { get; set; }
    }
}