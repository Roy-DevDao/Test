using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using test2.DAO;
using test2.Data;
using test2.Models.DoctorModel;
using test2.Services;

namespace test2.Controllers
{
    //[Authorize(Roles = "2")]
    public class DoctorController : Controller
    {
        DocCareContext _context;
        private readonly ILogger<DoctorController> _logger;
        private readonly AppointmentDAO _appointmentDAO;
        private readonly PatientDao _patientDao;
        private readonly FeedbackDAO _feedbackDao;
        private readonly UserDAO _userDAO;
        private readonly CloudinaryService _cloudinaryService;

		public DoctorController(ILogger<DoctorController> logger, AppointmentDAO appointmentDAO, PatientDao patientDao, FeedbackDAO feedbackDao, DocCareContext ct, UserDAO ud, CloudinaryService cloudinaryService)
		{
			_logger = logger;
			_appointmentDAO = appointmentDAO;
			_patientDao = patientDao;
			_feedbackDao = feedbackDao;
			_context = ct;
			_userDAO = ud;
			_cloudinaryService = cloudinaryService;
			_cloudinaryService = cloudinaryService;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.CurrentUserId = User.FindFirst("Id")?.Value;
            }
            base.OnActionExecuting(context);
        }
        public IActionResult Feedback(string id, string? sortOrder = "asc")
        {
            // Lấy danh sách phản hồi của bác sĩ dựa trên Did
            var feedbacks = _feedbackDao.GetFeedbacksByDoctorId(id);

            // Chuyển đổi phản hồi thành danh sách BaseViewModel
            var feedbackViewModels = feedbacks.Select(f => new BaseViewModel
            {
                feedbackView = new FeedbackViewModel
                {
                    FeedbackId = f.FeedbackId,
                    PatientName = f.PidNavigation?.Name,
                    DateCmt = f.DateCmt,
                    Star = f.Star,
                    Description = f.Description
                }
            }).ToList();

            // Sắp xếp theo thứ tự tăng hoặc giảm sao
            feedbackViewModels = sortOrder == "desc"
                ? feedbackViewModels.OrderByDescending(f => f.feedbackView.Star).ToList()
                : feedbackViewModels.OrderBy(f => f.feedbackView.Star).ToList();

            // Truyền danh sách BaseViewModel vào View
            return View(feedbackViewModels);
        }



        public IActionResult Profile(string id)
        {
            var userId = User.FindFirst(ClaimTypes.Name)?.Value;

            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (userId == null)
            {
                return RedirectToAction("Login", "Home"); // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
            }

            // Log giá trị oid
            _logger.LogInformation("OID received in DoctorProfile: {Oid}", id);

            // Kiểm tra xem ID của người dùng có khớp với ID trong URL không
            if (userId != id)
            {
                _logger.LogWarning("User attempted to access a profile that does not belong to them: {UserId} tried to access {TargetId}", userId, id);
                return Forbid(); // Ngăn chặn truy cập nếu ID không khớp
            }

            // Lấy thông tin bác sĩ từ cơ sở dữ liệu bằng id
            var doctor = (from d in _context.Doctors
                          join a in _context.Accounts on d.Did equals a.Id
                          join s in _context.Specialties on d.SpecialtyId equals s.SpecialtyId // Join với bảng chuyên khoa
                          where d.Did == id
                          select new BaseViewModel
                          {
                              DId = d.Did,
                              Name = d.Name,
                              DoctorImg = d.DoctorImg,
                              doctorProfile = new DoctorProfileViewModel
                              {
                                  Username = a.Username,
                                  Email = a.Email,
                                  Role = a.Role,
                                  Status = a.Status,
                                  Phone = d.Phone,
                                  Gender = d.Gender,
                                  Dob = d.Dob,
                                  Position = d.Position,
                                  Specialty = s.SpecialtyName, // Lấy tên chuyên khoa từ bảng chuyên khoa
                                  Description = d.Description,
                                  Price = d.Price,
                              }
                          }).ToList();

            // Kiểm tra xem bác sĩ có tồn tại không
            if (doctor == null)
            {
                _logger.LogWarning("No doctor found with ID: {Oid}", id); // Log cảnh báo nếu không tìm thấy
                return RedirectToAction("Login", "Home"); // Redirect về trang Login
            }

            // Trả về view cùng với model bác sĩ
            return View(doctor);
        }

		[HttpPost]
		public async Task<IActionResult> UpdateProfile(List<BaseViewModel> model, IFormFile DoctorImageUpload)
		{
			if (model == null || model.Count == 0)
			{
				_logger.LogWarning("Model is null or empty.");
				return BadRequest("Model cannot be null or empty.");
			}

			var baseViewModel = model.First();
			var userId = User.FindFirst(ClaimTypes.Name)?.Value;

			_logger.LogInformation("Checking authorization with userId: {UserId} and model.DId: {TargetId}", userId, baseViewModel.DId);

			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(baseViewModel.DId) || userId != baseViewModel.DId)
			{
				_logger.LogWarning("Unauthorized access attempt by user {UserId} to update profile of {TargetId}", userId, baseViewModel.DId);
				return Forbid();
			}

			var doctorProfile = _context.Doctors.FirstOrDefault(d => d.Did == baseViewModel.DId);
			if (doctorProfile == null)
			{
				_logger.LogWarning("Doctor not found with ID: {DoctorId}", baseViewModel.DId);
				return NotFound();
			}

			doctorProfile.Name = baseViewModel.Name;
			doctorProfile.Gender = baseViewModel.doctorProfile.Gender;
			doctorProfile.Dob = baseViewModel.doctorProfile.Dob;
			doctorProfile.Position = baseViewModel.doctorProfile.Position;
			doctorProfile.Price = baseViewModel.doctorProfile.Price;
			doctorProfile.Description = baseViewModel.doctorProfile.Description;

			if (DoctorImageUpload != null && DoctorImageUpload.Length > 0)
			{
				var imageUrl = await _cloudinaryService.UploadImageAsync(DoctorImageUpload);
				if (imageUrl != null)
				{
					doctorProfile.DoctorImg = imageUrl;
				}
				else
				{
					ModelState.AddModelError("", "Có lỗi xảy ra khi tải ảnh lên Cloudinary. Vui lòng thử lại.");
					return View("Profile", model);
				}
			}

			try
			{
				_context.SaveChanges();
				_logger.LogInformation("Doctor profile updated successfully for ID: {DoctorId}", baseViewModel.DId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating doctor profile for ID: {DoctorId}", baseViewModel.DId);
				ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật hồ sơ. Vui lòng thử lại.");
				return View("Profole", model);
			}

			TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công.";
			return RedirectToAction("Profile", new { id = baseViewModel.DId });
		}

		[HttpPost]
		public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
		{
			// Lấy ID của người dùng hiện tại từ Claim
			var userId = User.FindFirst(ClaimTypes.Name)?.Value;

			// Kiểm tra xem người dùng đã đăng nhập chưa
			if (userId == null)
			{
				return RedirectToAction("Login", "Home"); // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
			}

			// Log thông tin
			_logger.LogInformation("User {UserId} requested to change password", userId);

			// Kiểm tra tính hợp lệ của dữ liệu nhập vào
			if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
			{
				TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin mật khẩu.";
				return RedirectToAction("Profile", new { id = userId });
			}

			// Kiểm tra mật khẩu mới và xác nhận mật khẩu có khớp không
			if (newPassword != confirmPassword)
			{
				TempData["ErrorMessage"] = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
				return RedirectToAction("Profile", new { id = userId });
			}

			// Lấy tài khoản của bác sĩ từ cơ sở dữ liệu
			var doctorAccount = (from d in _context.Doctors
								 join a in _context.Accounts on d.Did equals a.Id
								 where d.Did == userId
								 select a).FirstOrDefault();

			// Kiểm tra xem tài khoản có tồn tại không
			if (doctorAccount == null)
			{
				_logger.LogWarning("No account found for Doctor with ID: {DoctorId}", userId);
				TempData["ErrorMessage"] = "Không tìm thấy tài khoản của bác sĩ.";
				return RedirectToAction("Profile", new { id = userId });
			}

			// Kiểm tra mật khẩu hiện tại có khớp không (giả sử mật khẩu được lưu ở dạng hash)
			if (!BCrypt.Net.BCrypt.Verify(currentPassword, doctorAccount.Password))
			{
				TempData["ErrorMessage"] = "Mật khẩu hiện tại không chính xác.";
				return RedirectToAction("Profile", new { id = userId });
			}

			// Cập nhật mật khẩu mới (sau khi đã hash) và lưu vào cơ sở dữ liệu
			doctorAccount.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

			try
			{
				await _context.SaveChangesAsync();
				_logger.LogInformation("Password changed successfully for Doctor with ID: {DoctorId}", userId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error changing password for Doctor with ID: {DoctorId}", userId);
				TempData["ErrorMessage"] = "Có lỗi xảy ra khi thay đổi mật khẩu. Vui lòng thử lại.";
				return RedirectToAction("Profile", new { id = userId });
			}

			TempData["SuccessMessage"] = "Mật khẩu đã được thay đổi thành công.";
			return RedirectToAction("Profile", new { id = userId });
		}
		public IActionResult ViewAppointment(string id)
        {
            var userId = User.FindFirst(ClaimTypes.Name)?.Value;

            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (userId == null)
            {
                return RedirectToAction("Login", "Home"); // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
            }

            // Log giá trị oid
            _logger.LogInformation("OID received in DoctorProfile: {Oid}", id);

            // Kiểm tra xem ID của người dùng có khớp với ID trong URL không
            if (userId != id)
            {
                _logger.LogWarning("User attempted to access a profile that does not belong to them: {UserId} tried to access {TargetId}", userId, id);
                return Forbid(); // Ngăn chặn truy cập nếu ID không khớp
            }

            // Lấy các cuộc hẹn cho bác sĩ có ID được truyền vào
            var appointment = _appointmentDAO.GetDoctorAppointments(id);

            if (appointment == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy
            }

            return View(appointment); // Trả về View với thông tin cuộc hẹn
        }

        public IActionResult ViewAppointmentDetail(string appointmentDetail)
        {
            if (string.IsNullOrEmpty(appointmentDetail))
            {
                return BadRequest("Appointment detail is missing.");
            }

            var appointmentDetailViewModel = _appointmentDAO.GetAppointmentDetailById(appointmentDetail);

            if (appointmentDetailViewModel == null)
            {
                return NotFound("Appointment not found.");
            }

            // Tạo danh sách BaseViewModel và gán appointmentDetail vào
            var baseViewModel = new BaseViewModel
            {
                appointmentDetail = appointmentDetailViewModel
            };

            // Truyền danh sách vào View
            return View("ViewAppointmentDetail", new List<BaseViewModel> { baseViewModel });
        }

        public IActionResult ViewPatient(string id)
        {
            // Lấy danh sách bệnh nhân dựa trên bác sĩ có Did = id
            var patients = _patientDao.GetPatientsByDoctorId(id);

            // Gói dữ liệu bệnh nhân vào BaseViewModel
            var baseViewModelList = patients.Select(p => new BaseViewModel
            {
                patientView = p  // Gán từng PatientViewModel vào BaseViewModel
            }).ToList();

            // Gán ID của bác sĩ vào ViewData để hiển thị trên giao diện
            ViewData["DoctorId"] = id;

            // Truyền danh sách BaseViewModel vào View
            return View(baseViewModelList);
        }



        public IActionResult ViewPatientDetail(string pid, string tab = "profile")
        {
            // Tìm bệnh nhân theo pid, bao gồm các đơn hàng và tùy chọn liên quan
            var patient = _context.Patients
                .Include(p => p.Orders)
                .ThenInclude(o => o.Option)
                .FirstOrDefault(p => p.Pid == pid);

            // Nếu bệnh nhân không tồn tại, trả về lỗi 404
            if (patient == null)
            {
                return NotFound();
            }

            // Tạo model chi tiết bệnh nhân
            var patientDetail = new PatientDetailViewModel
            {
                Pid = patient.Pid,
                Name = patient.Name,
                Phone = patient.Phone,
                Email = patient.PidNavigation?.Email,
                Dob = patient.Dob,
                Appointments = patient.Orders.Select(o => new PatientDetailViewModel.AppointmentViewModel
                {
                    Date = o.DateOrder?.ToString("yyyy-MM-dd"),
                    Time = o.DateOrder?.ToString("HH:mm"),
                    Status = o.Status ?? "N/A"
                }).ToList()
            };

            ViewBag.ActiveTab = tab; // Chuyển tab (profile/appointment)

            // Gói dữ liệu chi tiết bệnh nhân vào BaseViewModel
            var baseViewModel = new BaseViewModel
            {
                patientDetail = patientDetail
            };

            // Truyền danh sách BaseViewModel vào View
            return View("ViewPatientDetail", new List<BaseViewModel> { baseViewModel });
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}