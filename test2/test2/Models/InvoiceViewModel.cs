namespace test2.Models
{
    public class InvoiceViewModel
    {
        public string OrderId { get; set; }
        public string? Pid { get; set; }
        public string PatientName { get; set; }
        public DateOnly? PatientDOB { get; set; }
        public string PatientPhone { get; set; }
        public string PatientEmail { get; set; }
        public string DoctorName { get; set; }
        public string DoctorSpecialty { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public double? TotalAmount { get; set; }
    }
}
