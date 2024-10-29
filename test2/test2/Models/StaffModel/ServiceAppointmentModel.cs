namespace test2.Models.StaffModel
{
    public class ServiceAppointmentModel
    {
        public string AppointmentId { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string SpecialtyName { get; set; } // Added this property
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; }
    }
}
