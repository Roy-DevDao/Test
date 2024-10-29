using Microsoft.EntityFrameworkCore;
using test2.Data;
using test2.Models.DoctorModel;

namespace test2.DAO
{
    public class AppointmentDAO
    {
        private readonly DocCareContext _context;

        public AppointmentDAO(DocCareContext context)
        {
            _context = context;
        }

        public List<BaseViewModel> GetDoctorAppointments(string doctorId)
        {
            // Kiểm tra đầu vào doctorId để ngăn chặn các cuộc gọi cơ sở dữ liệu không cần thiết
            if (string.IsNullOrEmpty(doctorId))
            {
                return new List<BaseViewModel>();
            }

            // Lấy danh sách cuộc hẹn cho bác sĩ
            return _context.Options
                .Where(option => option.Did == doctorId)  // Lọc theo bác sĩ
                .Include(option => option.Orders)           // Bao gồm thông tin đơn hàng
                    .ThenInclude(order => order.PidNavigation) // Lấy thông tin bệnh nhân
                .Include(option => option.DidNavigation)    // Lấy thông tin bác sĩ
                .SelectMany(option => option.Orders, (option, order) => new BaseViewModel
                {
                    DId = option.Did,                // Thêm ID bác sĩ
                    Name = option.DidNavigation != null ? option.DidNavigation.Name : null, // Kiểm tra null
                    DoctorImg = option.DidNavigation != null ? option.DidNavigation.DoctorImg : null, // Kiểm tra null
                    appointmentlist = new AppointmentViewModel
                    {
                        AppointmentId = order.Oid,
                        PatientName = order.PidNavigation != null ? order.PidNavigation.Name : null, // Kiểm tra null
                        PatientImage = order.PidNavigation != null ? order.PidNavigation.PatientImg : null,
                        DateOrder = order.DateOrder,
                        Status = order.Status
                    }
                })
                .ToList();
        }

        public AppointmentDetailViewModel GetAppointmentDetailById(string appointmentId)
        {
            var appointment = _context.Orders
                .Include(o => o.PidNavigation)
                .Include(o => o.Option)
                    .ThenInclude(op => op.DidNavigation)
                    .ThenInclude(d => d.Specialty)
                .FirstOrDefault(o => o.Oid == appointmentId);

            if (appointment == null) return null;

            return new AppointmentDetailViewModel
            {
                AppointmentId = appointment.Oid,
                PatientName = appointment.PidNavigation?.Name,
                PatientImage = appointment.PidNavigation?.PatientImg,
                PatientPhone = appointment.PidNavigation?.Phone,
                Specialty = appointment.Option?.DidNavigation?.Specialty?.SpecialtyName,
                Price = appointment.Option?.DidNavigation?.Price,
                DateOrder = appointment.DateOrder,
                Symptom = appointment.Symptom,
                Status = appointment.Status
            };
        }






        // Lấy chi tiết cuộc hẹn theo Order Id (Oid)
        //public Order GetAppointmentDetailById(string AppointmentId)
        //{
        //    var appointment = _context.Orders
        //        .Where(o => o.Oid == AppointmentId)
        //        .Select(o => new Order
        //        {
        //            Oid = o.Oid,
        //            PidNavigation = o.PidNavigation,
        //            Option = o.Option,
        //            DateOrder = o.DateOrder,
        //            Status = o.Status,
        //            Symptom = o.Symptom
        //        })
        //        .FirstOrDefault();

        //    // Nếu không tìm thấy cuộc hẹn, trả về cuộc hẹn đầu tiên trong danh sách
        //    if (appointment == null)
        //    {
        //        return _context.Orders
        //            .Select(o => new Order
        //            {
        //                Oid = o.Oid,
        //                PidNavigation = o.PidNavigation,
        //                Option = o.Option,
        //                DateOrder = o.DateOrder,
        //                Status = o.Status,
        //                Symptom = o.Symptom
        //            })
        //            .FirstOrDefault();
        //    }

        //    return appointment;
        //}

    }
}