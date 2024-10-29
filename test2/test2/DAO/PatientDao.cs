using test2.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using test2.Models.DoctorModel;

namespace test2.DAO
{
    public class PatientDao
    {
        private readonly DocCareContext dc;

        public PatientDao(DocCareContext context)
        {
            dc = context;
        }

        // Lấy tất cả bệnh nhân của bác sĩ theo Did
        public List<PatientViewModel> GetPatientsByDoctorId(string did)
        {
            return dc.Patients
                .Include(p => p.Orders) // Gồm cả đơn hàng của bệnh nhân
                .ThenInclude(o => o.Option) // Bao gồm Option
                .Include(p => p.PidNavigation) // Bao gồm thông tin tài khoản bệnh nhân
                .Where(p => p.Orders.Any(o => o.Option != null && o.Option.Did == did))
                .Select(p => new PatientViewModel
                {
                    Pid = p.Pid,
                    Name = p.Name,
                    Phone = p.Phone,
                    Email = p.PidNavigation != null ? p.PidNavigation.Email : null, // Kiểm tra null ở đây
                    DateOrder = p.Orders.FirstOrDefault() != null
                                ? p.Orders.FirstOrDefault().DateOrder
                                : null, // Kiểm tra null
                    Dob = p.Dob
                })
                .ToList();
        }



        // Lấy chi tiết một bệnh nhân theo ID
        public Patient? GetPatientById(string pid)
        {
            return dc.Patients
                .Include(p => p.Orders) // Bao gồm Orders của bệnh nhân
                .ThenInclude(o => o.Option) // Bao gồm Option của từng Order
                .Include(p => p.PidNavigation) // Bao gồm thông tin tài khoản của bệnh nhân
                .FirstOrDefault(p => p.Pid == pid);
        }
    }
}