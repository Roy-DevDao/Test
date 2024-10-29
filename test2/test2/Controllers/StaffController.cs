using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using test2.DAO;
using test2.Data;
using test2.Models;
using test2.Models.StaffModel;

namespace test2.Controllers
{
    public class StaffController : Controller
    {
        private readonly DocCareContext dc;

        private readonly ILogger<StaffController> _logger;
        private readonly UserDAO _userDAO;
        private readonly StaffDAO staffDAO;

        public StaffController(ILogger<StaffController> logger, DocCareContext dc, UserDAO userDAO, StaffDAO staffDAO)
        {
            _logger = logger;
            this.dc = dc;
            _userDAO = userDAO;
            this.staffDAO = staffDAO;
        }
		//-------------------------------------------------------------------------------------------------------------

		public IActionResult Profile(string id)
		{
			var userId = User.FindFirst(ClaimTypes.Name)?.Value;

			// Kiểm tra xem người dùng đã đăng nhập chưa
			if (userId == null)
			{
				return RedirectToAction("Login", "Home"); // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
			}

			_logger.LogInformation("OID received in Profile: {Oid}", id); // Log giá trị oid

			if (userId != id)
			{
				_logger.LogWarning("User attempted to access a profile that does not belong to them: {UserId} tried to access {TargetId}", userId, id);
			}

			// Fetch patient details from the database using the oid
			var staff = _userDAO.GetLoggedInUser(User);

			if (staff == null)
			{
				_logger.LogWarning("No patient found with OID: {Oid}", id); // Log cảnh báo nếu không tìm thấy
				return RedirectToAction("Login", "Home");
			}

			// Pass the patient data to the view
			return View(staff);
		}

		[HttpPost]
		public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string RePassword)
		{
			// Kiểm tra tính hợp lệ của dữ liệu nhập vào
			if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(RePassword))
			{
				TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin mật khẩu.";
				return RedirectToAction("Profile");
			}

			// Kiểm tra mật khẩu xác nhận có khớp với mật khẩu mới hay không
			if (newPassword != RePassword)
			{
				TempData["ErrorMessage"] = "Mật khẩu xác nhận không khớp.";
				return RedirectToAction("Profile");
			}

			// Lấy thông tin tài khoản của người dùng hiện tại
			var accountId = User.Identity.Name; // Giả sử đây là ID tài khoản hoặc một định danh duy nhất
			var account = await dc.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);

			//if (account == null || !BCrypt.Net.BCrypt.Verify(oldPassword, account.Password))
			if (account != null && account.Password == oldPassword)
			{
				// Trường hợp mật khẩu cũ không đúng hoặc tài khoản không tồn tại
				TempData["ErrorMessage"] = "Mật khẩu cũ không đúng hoặc không tìm thấy tài khoản.";
				return RedirectToAction("Profile");
			}

			// Cập nhật mật khẩu mới sau khi đã mã hóa
			account.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
			await dc.SaveChangesAsync();

			// Thông báo đổi mật khẩu thành công
			TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
			return RedirectToAction("Profile");
		}

		public IActionResult AppoitmentList(string search_doctor = "", string sortColumn = "AppointmentId", string sortDirection = "asc", int pageNumber = 1)
        {
            int pageSize = 10;

            // Base query
            var appointmentsQuery = dc.Orders.Include(o => o.PidNavigation)
                                             .Include(o => o.Option)
                                             .ThenInclude(opt => opt.DidNavigation)
                                             .AsQueryable();

            // Search by doctor's name
            if (!string.IsNullOrEmpty(search_doctor))
            {
                appointmentsQuery = appointmentsQuery.Where(o => o.Option.DidNavigation.Name.Contains(search_doctor));
            }


            // Apply sorting
            switch (sortColumn)
            {
                case "PatientName":
                    appointmentsQuery = (sortDirection == "asc") ? appointmentsQuery.OrderBy(o => o.PidNavigation.Name) : appointmentsQuery.OrderByDescending(o => o.PidNavigation.Name);
                    break;
                case "DoctorName":
                    appointmentsQuery = (sortDirection == "asc") ? appointmentsQuery.OrderBy(o => o.Option.DidNavigation.Name) : appointmentsQuery.OrderByDescending(o => o.Option.DidNavigation.Name);
                    break;
                case "AppointmentDate":
                    appointmentsQuery = (sortDirection == "asc") ? appointmentsQuery.OrderBy(o => o.Option.DateWork) : appointmentsQuery.OrderByDescending(o => o.Option.DateWork);
                    break;
                default:
                    appointmentsQuery = (sortDirection == "asc") ? appointmentsQuery.OrderBy(o => o.Oid) : appointmentsQuery.OrderByDescending(o => o.Oid);
                    break;
            }

            // Pagination
            var appointments = appointmentsQuery.Select(o => new AppointmentViewModel
            {
                AppointmentId = o.Oid,
                PatientName = o.PidNavigation.Name,
                DoctorName = o.Option.DidNavigation.Name,
                //AppointmentDate = o.Option.DateExam ?? DateTime.MinValue,
                Status = o.Status
            })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            // Total pages calculation
            var totalAppointments = appointmentsQuery.Count();
            ViewBag.TotalPages = (int)Math.Ceiling(totalAppointments / (double)pageSize);
            ViewBag.PageNumber = pageNumber;

            // Preserve sorting and filtering
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;
            ViewBag.SearchDoctor = search_doctor;

            return View(appointments);
        }






        //-------------------------------------------------------------------------------------------------------------

        public IActionResult AppointmentDetail(string id)
        {
            // Retrieve the appointment details based on the appointment ID (OId)
            var appointment = dc.Orders
                .Include(o => o.PidNavigation) // Patient details
                .Include(o => o.Option)
                    .ThenInclude(opt => opt.DidNavigation) // Doctor details
                .ThenInclude(d => d.Specialty) // Include doctor's specialization
                .Where(o => o.Oid == id)
                .Select(o => new AppointmentDetailViewModel
                {
                    AppointmentId = o.Oid,
                    PatientName = o.PidNavigation.Name,
                    PatientPhone = o.PidNavigation.Phone,
                    PatientGender = o.PidNavigation.Gender,
                    PatientDOB = o.PidNavigation.Dob,
                    PatientImage = o.PidNavigation.PatientImg,
                    DoctorName = o.Option.DidNavigation.Name,
                    DoctorPhone = o.Option.DidNavigation.Phone,
                    DoctorSpecialization = o.Option.DidNavigation.Specialty.SpecialtyName,
                    DoctorImage = o.Option.DidNavigation.DoctorImg,
                    DoctorGender = o.Option.DidNavigation.Gender,
                    AppointmentDate = o.Option.DateWork.HasValue ? o.Option.DateWork.Value : DateTime.MinValue,
                    AppointmentTime = o.Option.DateWork.HasValue ? o.Option.DateWork.Value.ToString("hh:mm tt") : "N/A",
                    Status = o.Option.Status,
                    Fee = o.Option.DidNavigation.Price ?? 0,
                    SupportingStaff = "Bảo Staff",  // Static for now
                    ConsultationInfo = o.Symptom,  // Store examination details
                })
                .FirstOrDefault();

            // If appointment is not found, return NotFound or a similar view
            if (appointment == null)
            {
                return NotFound();
            }

            // Pass the data to the view
            return View(appointment);
        }


        //-------------------------------------------------------------------------------------------------------------

        public IActionResult ServiceAppointList(string search_service = "", string sortColumn = "AppointmentId", string sortDirection = "asc", string status = "all", int pageNumber = 1)
        {
            int pageSize = 10;

            // Base query for service appointments
            var appointmentsQuery = dc.Orders.Include(o => o.PidNavigation)
                .Include(o => o.Option)
                    .ThenInclude(opt => opt.DidNavigation)
                    .ThenInclude(d => d.Specialty)
                .AsQueryable();

            // Search by service name
            if (!string.IsNullOrEmpty(search_service))
            {
                appointmentsQuery = appointmentsQuery
                    .Where(o => o.Option.DidNavigation.Specialty != null &&
                                o.Option.DidNavigation.Specialty.SpecialtyName != null &&
                                o.Option.DidNavigation.Specialty.SpecialtyName.Contains(search_service.Trim()));
            }

            // Filter by status
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                appointmentsQuery = appointmentsQuery.Where(o => o.Status == status);
            }

            // Sorting functionality
            switch (sortColumn)
            {
                case "PatientName":
                    appointmentsQuery = sortDirection == "asc" ?
                        appointmentsQuery.OrderBy(o => o.PidNavigation.Name) :
                        appointmentsQuery.OrderByDescending(o => o.PidNavigation.Name);
                    break;
                case "SpecialtyName":
                    appointmentsQuery = sortDirection == "asc" ?
                        appointmentsQuery.OrderBy(o => o.Option.DidNavigation.Specialty.SpecialtyName) :
                        appointmentsQuery.OrderByDescending(o => o.Option.DidNavigation.Specialty.SpecialtyName);
                    break;
                case "AppointmentDate":
                    appointmentsQuery = sortDirection == "asc" ?
                        appointmentsQuery.OrderBy(o => o.Option.DateWork) :
                        appointmentsQuery.OrderByDescending(o => o.Option.DateWork);
                    break;
                case "DoctorName":
                    appointmentsQuery = sortDirection == "asc" ?
                        appointmentsQuery.OrderBy(o => o.Option.DidNavigation.Name) :
                        appointmentsQuery.OrderByDescending(o => o.Option.DidNavigation.Name);
                    break;
                default:
                    appointmentsQuery = sortDirection == "asc" ?
                        appointmentsQuery.OrderBy(o => o.Oid) :
                        appointmentsQuery.OrderByDescending(o => o.Oid);
                    break;
            }

            // Select fields and convert to ServiceAppointmentModel
            var appointments = appointmentsQuery.Select(o => new ServiceAppointmentModel
            {
                AppointmentId = o.Oid,
                PatientName = o.PidNavigation.Name,
                DoctorName = o.Option.DidNavigation.Name,
                SpecialtyName = o.Option.DidNavigation.Specialty.SpecialtyName,
                //AppointmentDate = o.Option.DateExam.HasValue ? o.Option.DateExam.Value : DateTime.MinValue,
                Status = o.Status
            })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            // Total appointments count for pagination
            var totalAppointments = appointmentsQuery.Count();
            ViewBag.TotalPages = (int)Math.Ceiling(totalAppointments / (double)pageSize);
            ViewBag.PageNumber = pageNumber;

            // Pass search, filter, sort values to the view
            ViewBag.SearchService = search_service;
            ViewBag.Status = status;
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortDirection = sortDirection;

            return View(appointments);
        }

        //-------------------------------------------------------------------------------------------------------------

        public IActionResult ServiceAppointDetail(string id)
        {
            // Lấy thông tin cuộc hẹn dựa trên ID (OId)
            var appointment = dc.Orders
                .Include(o => o.PidNavigation) // Thông tin bệnh nhân
                .Include(o => o.Option) // Thông tin dịch vụ
                    .ThenInclude(opt => opt.DidNavigation) // Thông tin bác sĩ
                .ThenInclude(d => d.Specialty) // Thông tin chuyên khoa
                .Where(o => o.Oid == id)
                .Select(o => new ServiceAppointmentDetailViewModel
                {
                    AppointmentId = o.Option.OptionId,
                    PatientName = o.PidNavigation.Name,
                    PatientPhone = o.PidNavigation.Phone,
                    PatientGender = o.PidNavigation.Gender,
                    PatientDOB = o.PidNavigation.Dob,
                    PatientImage = o.PidNavigation.PatientImg,
                    SpecialtyName = o.Option.DidNavigation.Specialty.SpecialtyName, // Tên chuyên khoa
                    SpecialtyImage = o.Option.DidNavigation.Specialty.SpecialtyImg, // Hình ảnh chuyên khoa
                    AppointmentDate = o.Option.DateWork.HasValue ? o.Option.DateWork.Value : DateTime.MinValue,
                    AppointmentTime = o.Option.DateWork.HasValue ? o.Option.DateWork.Value.ToString("hh:mm tt") : "N/A",
                    Status = o.Status,
                    Fee = o.Option.DidNavigation.Price ?? 0,
                    SupportingStaff = "Nguyễn Văn C", // Tên nhân viên hỗ trợ, có thể thay đổi sau
                    ConsultationInfo = o.Symptom // Thông tin tư vấn
                })
                .FirstOrDefault();

            // Nếu không tìm thấy cuộc hẹn, trả về NotFound hoặc một view tương tự
            if (appointment == null)
            {
                return NotFound();
            }

            // Truyền dữ liệu đến view
            return View(appointment);
        }

        //-------------------------------------------------------------------------------------------------------------

        public IActionResult Schedule(string doctorId, DateTime? selectedDate)
        {
            // Nếu không có ngày được chọn, mặc định là tuần từ 07/11/2024 đến 13/11/2024
            DateTime startDate = selectedDate.HasValue ? selectedDate.Value : new DateTime(2024, 11, 7);
            DateTime endDate = startDate.AddDays(6); // Lấy tuần từ ngày đã chọn

            // Lấy lịch làm việc của bác sĩ theo khoảng thời gian đã chọn
            var schedules = dc.Options
                .Where(opt => opt.Did == doctorId && opt.DateWork >= startDate && opt.DateWork <= endDate)
                .ToList();

            // Lấy danh sách các bác sĩ để hiển thị trong dropdown
            var doctors = dc.Doctors.ToList();

            // Truyền dữ liệu bác sĩ và lịch làm việc sang view
            ViewBag.Schedules = schedules;
            ViewBag.SelectedDoctor = doctorId; // Để biết bác sĩ nào đã được chọn
            ViewBag.SelectedDate = startDate;  // Để hiển thị lại ngày đã chọn

            return View(doctors);
        }

        [HttpPost]
        public IActionResult UpdateSchedule([FromBody] List<ScheduleUpdateModel> scheduleUpdates)
        {
            try
            {
                if (scheduleUpdates == null || !scheduleUpdates.Any())
                {
                    return Json(new { success = false, message = "No schedule data received" });
                }

                foreach (var update in scheduleUpdates)
                {
                    Console.WriteLine($"DoctorId: {update.DoctorId}, Date: {update.Date}, Time: {update.Time}"); // Log incoming data for validation

                    // Convert time string to TimeSpan
                    if (!TimeSpan.TryParse(update.Time, out TimeSpan parsedTime))
                    {
                        return Json(new { success = false, message = $"Invalid time format: {update.Time}" });
                    }

                    var updateDateTime = update.Date.Add(parsedTime);
                    var schedule = dc.Options.FirstOrDefault(opt =>
                        opt.Did == update.DoctorId &&
                        opt.DateWork.HasValue &&
                        opt.DateWork.Value == updateDateTime);

                    if (schedule != null)
                    {
                        // Nếu trạng thái là "Busy" chuyển sang "border-gray", thì xóa đối tượng Option khỏi database
                        if (schedule.Status == "Busy")
                        {
                            dc.Options.Remove(schedule);
                        }
                        else
                        {
                            schedule.Status = "Busy";
                        }
                    }
                    else
                    {
                        // If no schedule exists, create a new one with status "Busy"
                        dc.Options.Add(new Option
                        {
                            OptionId = Guid.NewGuid().ToString(),
                            Did = update.DoctorId,
                            DateWork = updateDateTime,
                            Status = "Busy"
                        });
                    }
                }

                dc.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); // Log detailed error for diagnosis
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateAppointmentStatus(string appointmentId, string newStatus = "Fail")
        {
            _logger.LogInformation($"Attempting to update appointment status. Appointment ID: {appointmentId}, New Status: {newStatus}");

            bool isUpdated = staffDAO.UpdateAppointmentStatus(appointmentId, newStatus);
            if (isUpdated)
            {
                TempData["Message"] = "Appointment status updated successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to update appointment status.";
            }
            return RedirectToAction("AppointmentDetail", new { id = appointmentId });
        }


        // Cập nhật trạng thái từ form ở trang ServiceAppointDetail
        [HttpPost]
        public IActionResult ServiceAppointDetailUpdate(string appointmentId, string newStatus)
        {
            _logger.LogInformation($"Attempting to update appointment status. Appointment ID: {appointmentId}, New Status: {newStatus}");

            bool isUpdated = staffDAO.UpdateAppointmentStatus(appointmentId, newStatus);
            if (isUpdated)
            {
                TempData["Message"] = "Appointment status updated successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to update appointment status.";
            }
            return RedirectToAction("ServiceAppointDetail", new { id = appointmentId });
        }





        //-------------------------------------------------------------------------------------------------------------

        public IActionResult ContactList(string status = "all", int pageNumber = 1)
        {
            int pageSize = 10; // Số lượng liên hệ trên mỗi trang

            // Truy vấn danh sách liên hệ
            var contactsQuery = dc.Contacts.AsQueryable();

            // Lọc theo trạng thái nếu có
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                contactsQuery = contactsQuery.Where(c => c.Status == status);
            }

            // Đếm tổng số liên hệ
            var totalContacts = contactsQuery.Count();

            // Lấy danh sách liên hệ dựa trên trang hiện tại và kích thước trang
            var contacts = contactsQuery
                .Skip((pageNumber - 1) * pageSize)  // Bỏ qua các liên hệ của trang trước
                .Take(pageSize)  // Lấy số liên hệ tương ứng với trang hiện tại
                .Select(c => new ContactViewModel
                {
                    ContactId = c.ContactId,
                    FullName = c.Name,
                    Email = c.Email,
                    Description = c.Description,
                    Status = c.Status
                })
                .ToList();

            // Tính tổng số trang
            ViewBag.TotalPages = (int)Math.Ceiling(totalContacts / (double)pageSize);
            ViewBag.PageNumber = pageNumber; // Trang hiện tại
            ViewBag.Status = status; // Giữ lại trạng thái lọc để hiển thị

            return View(contacts);
        }

        public IActionResult ResolveContact(string id)
        {
            // Tìm liên hệ theo ID
            var contact = dc.Contacts.FirstOrDefault(c => c.ContactId == id);
            if (contact != null && contact.Status == "Pending")
            {
                // Cập nhật trạng thái thành "resolved"
                contact.Status = "Resolved";
                dc.SaveChanges(); // Lưu thay đổi vào database
            }

            // Quay trở lại trang ContactList
            return RedirectToAction("ContactList");
        }

        //-------------------------------------------------------------------------------------------------------------

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
