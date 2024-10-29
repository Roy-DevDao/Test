
using test2.Data;
namespace test2.Models.PatientModel
{
    public class PatientBaseViewModel
    {
        public string? PatientImg { get; set; }
        public string PId { get; set; } = null!;
        public string? Name { get; set; }

        public PatientProfileViewModel PatientProfile { get; set; }
        public DoctorScheduleViewModel DoctorSchedule { get; set; }
        public OrderResponseDto OrderResponse { get; set; }
        public List<test2.Data.Order>﻿ order { get; set; }
    }
}
