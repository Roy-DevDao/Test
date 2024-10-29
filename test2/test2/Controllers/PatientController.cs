using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mscc.GenerativeAI;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using test2.DAO;
using test2.Data;
using test2.Models.DoctorModel;
using test2.Models.Order;
using test2.Services;
using System.Security.Claims;
using test2.Models.PatientModel;
using test2.Models;
using Azure;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using MailKit.Search;

namespace test2.Controllers
{
    //[Authorize(Roles = "3")]
    public class PatientController : Controller
    {
        private readonly ILogger<PatientController> _logger;
        private readonly DocCareContext dc;
        private readonly IVnPayService _vnPayservice;
        private readonly UserDAO _userDAO;
        private IMomoService _momoService;
        private readonly CloudinaryService _cloudinaryService;

        public PatientController(ILogger<PatientController> logger, DocCareContext db, IVnPayService vnPayservice, UserDAO userDAO, IMomoService momoService, CloudinaryService cloudinaryService)
        {
            _logger = logger;
            dc = db;
            _vnPayservice = vnPayservice;
            _userDAO = userDAO;
            _momoService = momoService;
            _cloudinaryService = cloudinaryService;
        }



		//public IActionResult Profile(string id)
		//{
		//    var userId = User.FindFirst(ClaimTypes.Name)?.Value;

		//    // Kiểm tra xem người dùng đã đăng nhập chưa
		//    if (userId == null)
		//    {
		//        return RedirectToAction("Login", "Home"); // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
		//    }

		//    _logger.LogInformation("OID received in Profile: {Oid}", id); // Log giá trị oid

		//    if (userId != id)
		//    {
		//        _logger.LogWarning("User attempted to access a profile that does not belong to them: {UserId} tried to access {TargetId}", userId, id);
		//    }

		//    // Fetch patient details from the database using the oid
		//    var patient = (from p in dc.Patients
		//                   join a in dc.Accounts on p.Pid equals a.Id
		//                   where p.Pid == id
		//                   select new PatientProfileViewModel
		//                   {
		//                       PId = p.Pid,
		//                       Username = a.Username,
		//                       Email = a.Email,
		//                       Role = a.Role,
		//                       Status = a.Status,
		//                       Name = p.Name,
		//                       Phone = p.Phone,
		//                       Gender = p.Gender,
		//                       Dob = p.Dob,
		//                       PatientImg = p.PatientImg
		//                   }).FirstOrDefault();

		//    if (patient == null)
		//    {
		//        _logger.LogWarning("No patient found with OID: {Oid}", id); // Log cảnh báo nếu không tìm thấy
		//        return RedirectToAction("Login", "Home");
		//    }

		//    // Pass the patient data to the view
		//    return View(patient);
		//}
		public IActionResult Profile(string id)
		{
			_logger.LogInformation("OID received in Profile: {Oid}", id); // Log giá trị id

			if (!User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Login", "Home");
			}

			// Lấy thông tin bệnh nhân từ database
			var patient = (from p in dc.Patients
						   join a in dc.Accounts on p.Pid equals a.Id
						   where p.Pid == id
						   select new PatientBaseViewModel
						   {
                               PId = id,
                               PatientImg = p.PatientImg,
                               Name = p.Name,
							   PatientProfile = new PatientProfileViewModel
							   {
								   Patient = new UpdateProfileViewModel
								   {
									   PId = p.Pid,
									   Username = a.Username,
									   Email = a.Email,
									   Role = a.Role,
									   Status = a.Status,
									   Name = p.Name,
									   Phone = p.Phone,
									   Gender = p.Gender,
									   Dob = p.Dob,
									   PatientImg = p.PatientImg
								   }
							   }

						   }).FirstOrDefault();

			if (patient == null)
			{
				_logger.LogWarning("No patient found with ID: {Oid}", id); // Log cảnh báo nếu không tìm thấy
				return RedirectToAction("Login", "Home");
			}

			// Tạo đối tượng PatientPageViewModel và gán các giá trị cần thiết
			var model = new PatientBaseViewModel
			{
                PId = id,
                PatientImg = patient.PatientImg,
                Name = patient.Name,

				PatientProfile = new PatientProfileViewModel
				{
					Patient = patient.PatientProfile.Patient,
					ChangePassword = new ChangePasswordViewModel { PId = id }
				}
			};

			// Truyền PatientPageViewModel vào view
			return View(model);
		}

		[HttpPost]
        public async Task<IActionResult> UpdateProfile(PatientProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Trả về lại view với dữ liệu đã nhập để giữ nguyên thông tin nếu có lỗi
                return View("Profile", model);
            }

            // Xử lý cập nhật thông tin cá nhân
            var patient = await dc.Patients.FirstOrDefaultAsync(p => p.Pid == model.Patient.PId);
            if (patient != null)
            {
                patient.Name = model.Patient.Name;
                patient.Gender = model.Patient.Gender;
                patient.Dob = model.Patient.Dob;

                if (model.Patient.AvataUpload != null && model.Patient.AvataUpload.Length > 0)
                {
                    // Upload ảnh lên Cloudinary và lấy link
                    var imageUrl = await _cloudinaryService.UploadImageAsync(model.Patient.AvataUpload);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        patient.PatientImg = imageUrl; // Lưu link ảnh vào database
                    }
                }

                await dc.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin bệnh nhân.";
            }

            return RedirectToAction("Profile", new { id = model.Patient.PId });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(PatientProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Trả về lại view với dữ liệu đã nhập để giữ nguyên thông tin nếu có lỗi
                return View("Profile", model);
            }

            // Xử lý đổi mật khẩu
            var account = await dc.Accounts.FirstOrDefaultAsync(a => a.Id == model.ChangePassword.PId);
            if (account != null && BCrypt.Net.BCrypt.Verify(model.ChangePassword.OldPassword, account.Password))
            {
                if (model.ChangePassword.NewPassword == model.ChangePassword.ConfirmNewPassword)
                {
                    account.Password = BCrypt.Net.BCrypt.HashPassword(model.ChangePassword.NewPassword);
                    await dc.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Mật khẩu xác nhận không khớp.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Mật khẩu cũ không đúng hoặc không tìm thấy tài khoản.";
            }

            return RedirectToAction("Profile", new { id = model.ChangePassword.PId });
        }


		//      [HttpGet]
		//[Route("Patient/AppointmentHistory")]
		//public IActionResult AppointmentHistory()
		//{
		//	var isAuthenticated = User.Identity.IsAuthenticated;
		//	if (isAuthenticated)
		//	{
		//		var user = _userDAO.GetLoggedInUser(User) ?? new UserProfileViewModel();
		//		ViewBag.User = user;
		//	}
		//	var id = User.Identity.Name;
		//	if (!isAuthenticated)
		//	{
		//		return RedirectToAction("Login", "Home");
		//	}
		//	ViewBag.Specialties = dc.Specialties.ToList();
		//	var list = dc.Orders
		//		 .Include(o => o.Option)             // Include the Option navigation property
		//		 .ThenInclude(op => op.DidNavigation)
		//		 .ThenInclude(doctor => doctor.Specialty).Where(o => o.Pid == id)
		//		 .OrderByDescending(o => o.DateOrder)
		//		 .ToList();
		//	int itemsPerPage = 5;

		//          var patient = dc.Patients
		//              .Where(p => p.Pid == id)          
		//     .FirstOrDefault();

		//          var orderList = list.Take(itemsPerPage).ToList();
		//          var order = new PatientBaseViewModel
		//          {
		//              PId = id,
		//              PatientImg = patient.PatientImg,
		//              Name = patient.Name,
		//              order = orderList,
		//          };
		//	return View(order);
		//}

		//public IActionResult AppointmentHistory()
		//{
		//    var user = _userDAO.GetLoggedInUser(User) ?? new UserProfileViewModel();
		//    ViewBag.User = user;
		//    var isAuthenticated = User.Identity.IsAuthenticated;
		//    var id = User.Identity.Name;
		//    if (!isAuthenticated)
		//    {
		//        return RedirectToAction("Login", "Home");
		//    }
		//    ViewBag.Specialties = dc.Specialties.ToList();
		//    var list = dc.Orders
		//         .Include(o => o.Option)             // Include the Option navigation property
		//         .ThenInclude(op => op.DidNavigation)
		//         .ThenInclude(doctor => doctor.Specialty).Where(o => o.Pid == id)
		//         .OrderByDescending(o => o.DateOrder)
		//         .ToList();
		//    int itemsPerPage = 5;

		//    var orderList = list.Take(itemsPerPage).ToList();
		//    return View(orderList);
		//}

		public IActionResult AppointmentHistory()
		{
			var user = _userDAO.GetLoggedInUser(User) ?? new UserProfileViewModel();
			ViewBag.User = user;
			var isAuthenticated = User.Identity.IsAuthenticated;
			var id = User.Identity.Name;
			if (!isAuthenticated)
			{
				return RedirectToAction("Login", "Home");
			}
			ViewBag.Specialties = dc.Specialties.ToList();
			var list = dc.Orders
				 .Include(o => o.Option)             // Include the Option navigation property
				 .ThenInclude(op => op.DidNavigation)
				 .ThenInclude(doctor => doctor.Specialty)
				 .Where(o => o.Pid == id)
				 .OrderBy(o => o.DateOrder)          // Sắp xếp theo thứ tự ngày xa nhất ở đầu
				 .ToList();
			int itemsPerPage = 5;

			var orderList = list.Take(itemsPerPage).ToList();
			return View(orderList);
		}


		public JsonResult GetAppointmentByQuery(string query, bool pending, bool complete, bool cancel, List<string> listFilter, string start, string end, int page)
        {

            var orders = dc.Orders
                           .Include(o => o.Option)
                           .ThenInclude(op => op.DidNavigation)
                           .ThenInclude(doctor => doctor.Specialty)
                           .Where(o => o.Pid == User.Identity.Name)
                           .AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                orders = orders.Where(o => o.Option.DidNavigation.Name.Contains(query) || o.Option.DidNavigation.Specialty.SpecialtyName.Contains(query));  // Filter by status
            }

            if ((pending && cancel && complete) || (!pending && !complete && !cancel))
            {

            }
            else
            {
                if (!pending)
                {
                    orders = orders.Where(o => o.Option.Status != "Pending");
                }
                if (!complete)
                {
                    orders = orders.Where(o => o.Option.Status != "Complete");
                }
                if (!cancel)
                {
                    orders = orders.Where(o => o.Option.Status != "Cancel");
                }
            }
            if (listFilter != null && listFilter.Count > 0)
            {
                orders = orders.Where(o => listFilter.Contains(o.Option.DidNavigation.SpecialtyId)); // Example filtering by doctor
            }
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                DateTime startDate = DateTime.Parse(start);
                DateTime endDate = DateTime.Parse(end);
                if (startDate < endDate)
                {
                    orders = orders.Where(o => o.DateOrder >= startDate && o.DateOrder <= endDate);
                }

            }


            int itemsPerPage = 5;
            if (page == 0)
            {
                page = 1;
            }
            var orderList = orders.Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList();
            if (orderList.Count > 0)
            {

                List<OrderResponseDto> data = new List<OrderResponseDto>();
                for (int i = 0; i < orderList.Count; i++)
                {
                    OrderResponseDto ord = new OrderResponseDto
                    {
                        Id = orderList[i].Oid,
                        Name = orderList[i].Option.DidNavigation.Name,
                        Image = orderList[i].Option.DidNavigation.DoctorImg,
                        Specialty = orderList[i].Option.DidNavigation.Specialty.SpecialtyName,
                        Date = (DateTime)orderList[i].DateOrder,
                        Status = orderList[i].Option.Status
                    };



                    data.Add(ord);

                }
                var total = orders.Count();
                if (page == 1)
                {
                    return Json(new { data = data, total = total });
                }
                return Json(new { data = data });
            }
            return Json(new { total = -1 });

        }

        public IActionResult AppointmentDetail(string oid)
        {

            var isAuthenticated = User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                var user = _userDAO.GetLoggedInUser(User) ?? new UserProfileViewModel();
                ViewBag.User = user;
            } else
            {
                return RedirectToAction("Login", "Home");
            }
            // Fetch the order details including all necessary related information
            var order = dc.Orders
                          .Include(o => o.Option)
                          .ThenInclude(op => op.DidNavigation)
                          .ThenInclude(doctor => doctor.Specialty)
                          .Include(o => o.PidNavigation)
                          .Include(o => o.HealthRecords)
                          .ThenInclude(hr => hr.DidNavigation)
                          .FirstOrDefault(o => o.Oid == oid);

            if (order == null)
            {
                return NotFound(); // Handle the case when the order is not found
            }

            return View(order);  // Pass the order object to the view
        }





        [HttpGet]
        public IActionResult BookingAppointment(string doctorid)
        {
            var isAuthenticated = User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                var user = _userDAO.GetLoggedInUser(User) ?? new UserProfileViewModel();
                ViewBag.User = user;
            } else
            {
                return RedirectToAction("Login", "Home");
            }

            if (!string.IsNullOrWhiteSpace(doctorid))
            {

                // Fetch the doctor including their specialty
                Doctor doctor = dc.Doctors.Include(d => d.Specialty).FirstOrDefault(d => d.Did == doctorid);

                if (doctor != null)
                {
                    // Define the date range: today and the next 7 days
                    DateTime today = DateTime.Now.Date;  // Current date (no time part)
                    DateTime next7Days = today.AddDays(7);  // Next 7 days (including today)

                    // Get the schedule for the doctor in the next 7 days
                    var schedule = dc.Options
                        .Where(o => o.Did == doctor.Did && o.DateWork >= today && o.DateWork <= next7Days)
                        .ToList();


                    var viewModel = new DoctorScheduleViewModel
                    {
                        Doctor = doctor,
                        Schedule = schedule,
                        Today = today
                    };
                    //var viewModel = new PatientBaseViewModel
                    //{
                    //    DoctorSchedule = new DoctorScheduleViewModel
                    //    {
                    //        Doctor = doctor,
                    //        Schedule = schedule,
                    //        Today = today
                    //    }
                    //};
                    // You can pass 'schedule' to the view if needed
                    return View(viewModel);
                }
            }


            return View();
        }
        //[HttpPost]
        //public async Task<JsonResult> ProcessBooking(string doctorid, string desc, string time, string paymentMethod)
        //{
        //    if (!string.IsNullOrEmpty(doctorid) && !string.IsNullOrEmpty(time) && !string.IsNullOrEmpty(desc))
        //    {
        //        if (dc.Options.Any(o => o.Did == doctorid && o.DateWork == DateTime.Parse(time)))
        //        {
        //            return Json(new { error = "Slot was booked by another patient" });
        //        }
        //        else
        //        {
        //            Random random = new Random();
        //            int buff = random.Next(1000000, 9999999);
        //            string optid = "opt" + buff;
        //            string ordid = "ord" + buff;

        //            using (var transaction = dc.Database.BeginTransaction())
        //            {
        //                try
        //                {
        //                    // Tạo đối tượng Option
        //                    Option op = new Option
        //                    {
        //                        OptionId = optid,
        //                        Status = "Pending",
        //                        Did = doctorid,
        //                        DateWork = DateTime.Parse(time),
        //                    };

        //                    // Thêm vào bảng Option và lưu
        //                    dc.Options.Add(op);
        //                    await dc.SaveChangesAsync();

        //                    // Lấy giá của bác sĩ từ cơ sở dữ liệu
        //                    double amount = dc.Doctors
        //                    .Where(d => d.Did == doctorid)
        //                        .Select(d => d.Price)
        //                           .FirstOrDefault() ?? 0; // Nếu là null thì gán giá trị mặc định là 0


        //                    // Tạo đối tượng Order
        //                    Order order = new Order
        //                    {
        //                        Oid = ordid,
        //                        Pid = User.Identity.Name,
        //                        OptionId = op.OptionId,
        //                        DateOrder = DateTime.Now,
        //                        Symptom = desc,
        //                        Status = "Pending", // Trạng thái hóa đơn ban đầu
        //                    };

        //                    // Thêm vào bảng Order và lưu
        //                    dc.Orders.Add(order);
        //                    await dc.SaveChangesAsync();

        //                    // Chuyển hướng đến phương thức thanh toán với thông tin số tiền
        //                    string paymentUrl = "";
        //                    if (paymentMethod == "VNPay")
        //                    {
        //                        paymentUrl = Url.Action("VnPayment", new { orderId = order.Oid, amount });
        //                    }
        //                    else if (paymentMethod == "MoMo")
        //                    {
        //                        paymentUrl = Url.Action("MomoPayment", new { orderId = order.Oid, amount });
        //                    }

        //                    // Lưu transaction
        //                    transaction.Commit();

        //                    // Trả về URL thanh toán để chuyển hướng người dùng
        //                    return Json(new { success = true, paymentUrl });
        //                }
        //                catch (Exception ex)
        //                {
        //                    // Rollback transaction nếu có lỗi
        //                    transaction.Rollback();
        //                    Debug.WriteLine($"Error saving to database: {ex.Message}");
        //                    return Json(new { error = "Error saving to database" });
        //                }
        //            }
        //        }
        //    }
        //    else return Json(new { error = "Invalid data" });
        //}

        [HttpPost]
        public async Task<JsonResult> ProcessBooking(string doctorid, string desc, string time)
        {

            if (!string.IsNullOrEmpty(doctorid) && !string.IsNullOrEmpty(time) && !string.IsNullOrEmpty(desc))
            {
                if (dc.Options.Any(o => o.Did == doctorid && o.DateWork == DateTime.Parse(time) && o.Status != "Canceled"))
                {
                    return Json(new { err = "Slot was Book by other people" });
                }
                else
                {
                    Random random = new Random();
                    int buff = random.Next(1000000, 9999999);
                    string optid = "opt" + buff;
                    string ordid = "ord" + buff;
                    using (var transaction = dc.Database.BeginTransaction())
                    {
                        try
                        {
                            // Tạo đối tượng Option
                            Option op = new Option
                            {
                                OptionId = optid,
                                Status = "Unpaid",
                                Did = doctorid,
                                DateWork = DateTime.Parse(time),
                            };

                            // Thêm vào bảng Option và lưu
                            dc.Options.Add(op);
                            await dc.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

                            // Tạo đối tượng Order
                            Order order = new Order
                            {
                                Oid = ordid,
                                Pid = User.Identity.Name,
                                OptionId = op.OptionId,
                                DateOrder = DateTime.Now,
                                Status = "Unpaid",
                                Symptom = desc,
                            };

                            // Thêm vào bảng Order và lưu
                            dc.Orders.Add(order);
                            await dc.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

                            //Payment pay = new Payment
                            //{
                            //    PayId
                            //}
                            // Commit transaction
                            transaction.Commit();

                            return Json(new { success = true, orderId = ordid });
                        }
                        catch (Exception ex)
                        {
                            // Rollback transaction
                            transaction.Rollback();
                            Debug.WriteLine($"Error saving to database: {ex.Message}");
                            return Json(new { error = "Error saving to database" });
                        }
                    }


                }
            }
            else return Json(new { error = "Data is invalid" });
        }

        public async Task<JsonResult> CancelAppointment(string oid)
        {
            using (var transaction = dc.Database.BeginTransaction())
            {
                try
                {
                    Order order = dc.Orders.Include(o => o.Option).FirstOrDefault(o => o.Oid == oid);
                    if (order == null)
                    {
                        return Json(new { error = "Order is't found" });
                    }
                    if (order.Status != "Confirm" && order.Status != "Unpaid")
                    {
                        return Json(new { error = "Order can not be canceled" });

                    }
                    DateTime timenow = DateTime.Now.AddHours(5);
                    if (order.Option.DateWork < timenow)
                    {
                        return Json(new { error = "Your order exceed allowed time for canceling" });
                    }
                    order.Option.Status = "Canceled";
                    dc.Entry(order.Option).State = EntityState.Modified;
                    await dc.SaveChangesAsync();

                    // Update the status of the Order entity
                    order.Status = "Canceled";
                    dc.Entry(order).State = EntityState.Modified;

                    // Save the changes to both entities
                    await dc.SaveChangesAsync();



                    transaction.Commit();
                    return Json(new { success = true });


                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { error = "Cancel failed. Please call to hospital for detail" });
                }
            }
            return Json(new { error = "fsdf" });
        }

        [HttpPost]
        public async Task<JsonResult> SendMessage(string message, string doctorid, int star)
        {
            Random random = new Random();
            if (User.Identity.Name != null)
            {
                Patient patient = dc.Patients.FirstOrDefault(p => p.Pid == User.Identity.Name);
                if (patient != null)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        int buff = random.Next(1000000, 9999999);
                        string fbid = "f" + buff;
                        if (!dc.Feedbacks.Any(f => f.FeedbackId == fbid))
                        {
                            Feedback feedback = new Feedback
                            {
                                FeedbackId = fbid,
                                Did = doctorid,
                                Star = star,
                                Pid = User.Identity.Name,
                                Description = message,
                                DateCmt = DateTime.Now,
                                Name = patient.Name,

                            };
                            try
                            {
                                dc.Feedbacks.Add(feedback);
                                await dc.SaveChangesAsync();
                                return Json(new { success = true, name = feedback.Name, description = feedback.Description, star = feedback.Star, ngay = feedback.DateCmt.ToString(), image = patient.PatientImg });
                            }
                            catch (Exception ex)
                            {
                                return Json(new { error = "save feedback fail" });
                            }


                        }
                    }

                }


            }



            return Json(new { error = "user invalid" });
        }


		[Authorize]
		public IActionResult VnPaymentCallBack()
		{
			var response = _vnPayservice.PaymentExecute(Request.Query);

			// Kiểm tra response có hợp lệ không
			if (response == null || response.VnPayResponseCode != "00")
			{
				// Xử lý thanh toán thất bại
				var order = dc.Orders.Find(response.OrderId);
				if (order != null)
				{
					// Xóa Option liên quan
					var option = dc.Options.Find(order.OptionId);
					if (option != null)
					{
						dc.Options.Remove(option);
					}

					// Xóa Order
					dc.Orders.Remove(order);
				}

				dc.SaveChanges();
				TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}. Bạn sẽ quay lại trang chủ.";
				return RedirectToAction("PaymentFail", "Patient");
			}

			// Tiếp tục xử lý nếu thanh toán thành công
			var orderToUpdate = dc.Orders.Find(response.OrderId);
			if (orderToUpdate == null)
			{
				// Xử lý trường hợp không tìm thấy đơn hàng
				TempData["Message"] = "Đơn hàng không tồn tại.";
				return RedirectToAction("PaymentFail", "Patient"); // Redirect tới trang lỗi
			}

			var option1 = dc.Options.Find(orderToUpdate.OptionId);
			orderToUpdate.Status = "Confirm"; // Cập nhật trạng thái thành "Paid"

			if (option1 != null)
			{
				option1.Status = "Confirm";
			}

			Random random = new Random();
			int buff = random.Next(1000000, 9999999);
			Payment payment = new Payment
			{
				PayId = "pay" + buff,
				Oid = response.OrderId,
				Method = "VnPay",
				PayImg = "null", // Hình ảnh thanh toán nếu có
				DatePay = DateTime.Now // Thời gian thanh toán
			};

			dc.Payments.Add(payment); // Thêm bản ghi vào DbContext
			dc.SaveChanges(); // Lưu thay đổi

			TempData["Message"] = $"Thanh toán VNPay thành công! Bạn sẽ được chuyển đến lịch hẹn.";
			return RedirectToAction("Success", "Patient"); // Chuyển hướng đến trang lịch hẹn
		}

		public IActionResult VnPayment( string orderId, double total)
        {
            var isAuthenticated = User.Identity.IsAuthenticated;
            if (!isAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            var order = dc.Orders.Find(orderId);
            var vnPayModel = new VnPaymentRequestModel
            {
                Amount = total,
                CreatedDate = DateTime.Now,
                Description = "Thanh Toán Hoá Đơn",
                FullName = order.Pid,
                OrderId = orderId,
            };
            return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
        }

        //[Authorize]
        //public IActionResult VnPaymentCallBack()
        //{
        //    var response = _vnPayservice.PaymentExecute(Request.Query);

        //    if (response == null || response.VnPayResponseCode != "00")
        //    {
        //        TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}. Bạn sẽ quay lại trang chủ.";
        //        return RedirectToAction("Index", "Home");
        //    }



        //    TempData["Message"] = $"Thanh toán VNPay thành công! Bạn sẽ được chuyển đến lịch hẹn.";
        //    return RedirectToAction("AppointmentHistory", "Home"); // Chuyển hướng đến trang lịch hẹn
        //}

        //-----------------------------------------------------------------------------------------------------------------
        public IActionResult MomoPayment(string orderId, double total)
        {
            var user = _userDAO.GetLoggedInUser(User) ?? new UserProfileViewModel();
            ViewBag.User = user;
            var order = dc.Orders.Find(orderId);
            var option = dc.Options.Find(order.OptionId);
            var momoModel = new OrderInfoModel
            {
                FullName = User.Identity.Name,
                OrderId = orderId,
                Amount = total,
                OrderInfo = "Thanh toán hóa đơn",
            };
            option.Status = "Confirm";
            order.Status = "Confirm";
            Random random = new Random();
            int buff = random.Next(1000000, 9999999);
            Payment payment = new Payment
            {
                PayId = "pay" + buff,
                Oid = orderId,
                Method = "MoMo",
                PayImg = "null", // Hình ảnh thanh toán nếu có
                DatePay = DateTime.Now // Thời gian thanh toán

            };
            dc.Payments.Add(payment);
            dc.SaveChangesAsync();

           
            return View(momoModel);
        }
        //[HttpPost]
        //public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
        //{
        //    var response = await _momoService.CreatePaymentAsync(model);
        //    return Redirect(response.PayUrl);
        //}


        //[HttpGet]
        //public IActionResult MomoPaymentCallBack()
        //{

        //    var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);

        //    // Kiểm tra phản hồi từ MoMo
        //    if (response == null || string.IsNullOrEmpty(response.OrderId))
        //    {
        //        // Xử lý trường hợp không có thông tin đơn hàng
        //        TempData["ErrorMessage"] = "Không có thông tin đơn hàng. Thanh toán không thành công.";
        //        return RedirectToAction("AppointmentHistory");
        //    }

        //    // Kiểm tra mã trạng thái (giả sử "0" là thành công)
        //    if (response.ResponseCode != "0")
        //    {
        //        DeleteOrderAndRelatedData(response.OrderId);
        //        // Thanh toán thất bại
        //        TempData["ErrorMessage"] = $"Thanh toán thất bại: {response.ResponseCode}";
        //        return RedirectToAction("MomoPayment");
        //    }

        //    var orderToUpdate = dc.Orders.Find(response.OrderId);
        //    if (orderToUpdate != null)
        //    {
        //        orderToUpdate.Status = "Paid"; // Cập nhật trạng thái thành "Paid"
        //        var payment = new Payment
        //        {
        //            PayId = Guid.NewGuid().ToString(), // Tạo ID duy nhất cho Payment
        //            Oid = response.OrderId, // ID đơn hàng
        //            Method = "MoMo", // Hoặc phương thức thanh toán khác nếu cần
        //            PayImg = "null", // Hình ảnh thanh toán nếu có
        //            DatePay = DateTime.Now // Thời gian thanh toán
        //        };

        //        dc.Payments.Add(payment); // Thêm bản ghi vào DbContext

        //        dc.SaveChanges(); // Lưu thay đổi
        //    }

        //    // Nếu thanh toán thành công, lưu đơn hàng vào database
        //    // Gọi hàm lưu đơn hàng ở đây

        //    // Chuyển hướng tới view hiển thị thanh toán thành công
        //    return View(response); // Truyền response vào view
        //}

        //            switch (response.ResponseCode)
        //            {
        //                case "0":
        //                    var orderToUpdate = dc.Orders.Find(response.OrderId);
        //                    if (orderToUpdate != null)
        //                    {
        //                        orderToUpdate.Status = "Paid"; // Cập nhật trạng thái thành "Paid"
        //                        var payment = new Payment
        //                        {
        //                            PayId = Guid.NewGuid().ToString(), // Tạo ID duy nhất cho Payment
        //                            Oid = response.OrderId, // ID đơn hàng
        //                            Method = "MoMo", // Hoặc phương thức thanh toán khác nếu cần
        //                            PayImg = "null", // Hình ảnh thanh toán nếu có
        //                            DatePay = DateTime.Now // Thời gian thanh toán
        //                        };

        //        dc.Payments.Add(payment); // Thêm bản ghi vào DbContext

        //                        dc.SaveChanges(); // Lưu thay đổi

        //                        return RedirectToAction("PaymentSuccess", new
        //                        {
        //                            orderId = response.OrderId,
        //                            amount = response.Amount,
        //                            message = "Thanh toán thành công"
        //                        });
        //                    }
        //break;

        //            case "1006": // Giao dịch bị từ chối bởi người dùng
        //    return RedirectToAction("PaymentCancelled", new
        //    {
        //        orderId = response.OrderId,
        //        message = "Giao dịch đã bị hủy"
        //    });

        //default: // Các trường hợp lỗi khác
        //    return RedirectToAction("PaymentError", new
        //    {
        //        orderId = response.OrderId,
        //        errorCode = response.ResponseCode,
        //        message = response.Message
        //    });
        //}
        private async Task DeleteOrderAndRelatedData(string orderId)
        {
            using (var context = new DocCareContext())
            {
                // Xóa HealthRecord liên quan đến Order
                var healthRecords = context.HealthRecords.Where(hr => hr.Oid == orderId);
                context.HealthRecords.RemoveRange(healthRecords);

                // Tìm Order và lấy OptionId
                var order = await context.Orders.FindAsync(orderId);
                if (order != null)
                {
                    // Xóa Option liên quan
                    var option = await context.Options.FindAsync(order.OptionId);
                    if (option != null)
                    {
                        context.Options.Remove(option);
                    }

                    // Xóa Order
                    context.Orders.Remove(order);
                }

                // Lưu thay đổi
                await context.SaveChangesAsync();
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentAsync(model);
            return Redirect(response.PayUrl);
        }

        [HttpGet]
        public IActionResult MomoPaymentCallBack()
        {

            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
         
                return RedirectToAction("AppointmentHistory", "Patient");


        } 



        [HttpGet("Patient/Invoice/{orderId}")]
        public IActionResult Invoice(string orderId)
        {
            // Lấy thông tin người dùng đã đăng nhập
            var user = _userDAO.GetLoggedInUser(User) ?? new UserProfileViewModel();
            ViewBag.User = user;

            // Tìm đơn hàng theo orderId
            var order = dc.Orders.Find(orderId);

            // Kiểm tra xem đơn hàng có tồn tại không
            if (order == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy đơn hàng
            }

            // Kiểm tra xem OptionId có hợp lệ không
            if (string.IsNullOrEmpty(order.OptionId))
            {
                return BadRequest("OptionId is null or empty."); // Trả về lỗi nếu OptionId không hợp lệ
            }

            // Tìm thông tin tùy chọn
            var option = dc.Options.Find(order.OptionId);

            // Kiểm tra xem tùy chọn có tồn tại không
            if (option == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy tùy chọn
            }

            // Tìm bệnh nhân theo PId
            var patient = dc.Patients.Find(order.Pid);

            // Kiểm tra xem bệnh nhân có tồn tại không
            if (patient == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy bệnh nhân
            }

            // Tìm bác sĩ theo DId
            var doctor = dc.Doctors.Find(option.Did);

            // Kiểm tra xem bác sĩ có tồn tại không
            if (doctor == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy bác sĩ
            }

            // Tìm chuyên khoa theo SpecialtyId
            var specialty = dc.Specialties.Find(doctor.SpecialtyId);

            // Tạo mô hình hóa đơn
            var invoiceModel = new InvoiceViewModel
            {
                OrderId = order.Oid,
                PatientName = patient.Name,
                PatientDOB = patient.Dob,
                PatientPhone = patient.Phone,
                PatientEmail = user.Email,
                DoctorName = doctor.Name,
                DoctorSpecialty = specialty?.SpecialtyName ?? "Chưa xác định", // Nếu không tìm thấy chuyên khoa
                AppointmentDate = option.DateWork,
                TotalAmount = doctor.Price * 2000 // Tính tổng số tiền
            };

            // Trả về View với mô hình hóa đơn
            return View(invoiceModel);
        }


        //-----------------------------------------------------------------------------------------------------------------------------------

        public IActionResult BookingService()
        {
            var isAuthenticated = User.Identity.IsAuthenticated;
            if (!isAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            return View();  // This will render /Views/Staff/ServiceAppointList.cshtml
        }

       


        public IActionResult ServiceHistory()
        {
            var isAuthenticated = User.Identity.IsAuthenticated;
            if (!isAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            return View();  // This will render /Views/Staff/ServiceAppointDetail.cshtml
        }

        //[Authorize]
        //[HttpPost]
        //public IActionResult Checkout(CheckoutVM model, string payment = "COD")
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (payment == "Thanh toán VNPay")
        //        {
        //            var vnPayModel = new VnPaymentRequestModel
        //            {
        //                Amount = Cart.Sum(p => p.ThanhTien),
        //                CreatedDate = DateTime.Now,
        //                Description = $"{model.HoTen} {model.DienThoai}",
        //                FullName = model.HoTen,
        //                OrderId = new Random().Next(1000, 100000)
        //            };
        //            return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
        //        }

        //        var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;
        //        var khachHang = new KhachHang();
        //        if (model.GiongKhachHang)
        //        {
        //            khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
        //        }

        //        var hoadon = new HoaDon
        //        {
        //            MaKh = customerId,
        //            HoTen = model.HoTen ?? khachHang.HoTen,
        //            DiaChi = model.DiaChi ?? khachHang.DiaChi,
        //            DienThoai = model.DienThoai ?? khachHang.DienThoai,
        //            NgayDat = DateTime.Now,
        //            CachThanhToan = "COD",
        //            CachVanChuyen = "GRAB",
        //            MaTrangThai = 0,
        //            GhiChu = model.GhiChu
        //        };

        //        db.Database.BeginTransaction();
        //        try
        //        {

        //            db.Add(hoadon);
        //            db.SaveChanges();

        //            var cthds = new List<ChiTietHd>();
        //            foreach (var item in Cart)
        //            {
        //                cthds.Add(new ChiTietHd
        //                {
        //                    MaHd = hoadon.MaHd,
        //                    SoLuong = item.SoLuong,
        //                    DonGia = item.DonGia,
        //                    MaHh = item.MaHh,
        //                    GiamGia = 0
        //                });
        //            }
        //            db.AddRange(cthds);
        //            db.SaveChanges();
        //            db.Database.CommitTransaction();

        //            HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());

        //            return View("Success");
        //        }
        //        catch
        //        {
        //            db.Database.RollbackTransaction();
        //        }
        //    }

        //    return View(Cart);
        //}

        [Authorize]
        //public IActionResult PaymentFail()
        //{
        //    // Ghi nhận thông báo lỗi
        //    TempData["Message"] = "Thanh toán thất bại. Vui lòng thử lại.";
        //    // Chuyển hướng về trang chủ
        //    return RedirectToAction("Index", "Home"); // Giả sử 'Index' là phương thức trong 'HomeController'
        //}

        //[Authorize]
        //public IActionResult PaymentSuccess()
        //{
        //    // Chuyển hướng về trang lịch sử đặt chỗ (appointment history)
        //    return RedirectToAction("AppointmentHistory", "Appointment"); // Giả sử 'AppointmentHistory' là phương thức trong 'AppointmentController'
        //}


    //    public IActionResult PaymentSuccess(string orderId, string amount, string message)
    //    {
    //        var viewModel = new PaymentResultViewModel
    //        {
    //            OrderId = orderId,
    //            Amount = amount,
    //            Message = message,
    //            Status = "Success"
    //        };
    //        return View(viewModel);
    //    }

    //    public IActionResult PaymentCancelled(string orderId, string message)
    //    {
    //        var viewModel = new PaymentResultViewModel
    //        {
    //            OrderId = orderId,
    //            Message = message,
    //            Status = "Cancelled"
    //        };
    //        return View("PaymentError", viewModel);
    //    }

    //    public IActionResult PaymentError(string orderId, string errorCode, string message)
    //    {
    //        var viewModel = new PaymentResultViewModel
    //        {
    //            OrderId = orderId,
    //            ErrorCode = errorCode,
    //            Message = message,
    //            Status = "Error"
    //        };
    //        return View(viewModel);
    //    }
    //}

    //public class PaymentResultViewModel
    //{
    //    public string OrderId { get; set; }
    //    public string Amount { get; set; }
    //    public string Message { get; set; }
    //    public string Status { get; set; }
    //    public string ErrorCode { get; set; }
    //}

    //// Model cho giao dịch thanh toán
    //public class PaymentTransaction
    //{
    //    public string OrderId { get; set; }
    //    public string TransactionId { get; set; }
    //    public decimal Amount { get; set; }
    //    public string PaymentMethod { get; set; }
    //    public string PaymentType { get; set; }
    //    public PaymentStatus Status { get; set; }
    //    public DateTime CreatedAt { get; set; }
    //}

    //public enum PaymentStatus
    //{
    //    Success,
    //    Failed,
    //    Cancelled
    //}

    //public enum OrderStatus
    //{
    //    Pending,
    //    Paid,
    //    Cancelled,
    //    Failed
    //}


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
