using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using test2.Data;
using test2.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using test2.DAO;
using test2.Services;

namespace test2.Controllers
{
    public class HomeController : Controller
    {
        private readonly DocCareContext dc;
        private readonly ILogger<HomeController> _logger;
        private readonly UserDAO _userDAO;
        private readonly EmailService _emailService;
        private readonly TokenService _tokenService;

        public HomeController(ILogger<HomeController> logger, DocCareContext db, UserDAO userDAO, EmailService emailService, TokenService tokenService)
        {
            _logger = logger;
            dc = db;
            _userDAO = userDAO; // Khởi tạo UserDAO
            _emailService = emailService;
            _tokenService = tokenService;
        }
        //--------------------------------------------------------------------------------------------
        public IActionResult Index()
        {
            var isAuthenticated = User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                var user = _userDAO.GetLoggedInUser(User) ?? new UserProfileViewModel();
                ViewBag.User = user;
            }
            var doctors = dc.Doctors.Take(10).ToList();
            var specialties = dc.Specialties.Take(10).ToList();

            //truyen data thong qua view bag
            ViewBag.Doctors = doctors;
            ViewBag.Specialties = specialties;

            return View();

        }
        //--------------------------------------------------------------------------------------------

        //public IActionResult DoctorList(string gender = "all", string speciality = "all", string sort = "all", int pageNumber = 1)
        //{
        //    int pageSize = 9;

        //    var specialties = dc.Specialties.ToList();
        //    ViewBag.Specialties = specialties;

        //    var doctors = dc.Doctors.Include(d => d.Specialty).Include(d => d.Feedbacks).AsQueryable();

        //    // Lọc theo giới tính
        //    if (gender != "all")
        //    {
        //        bool isMale = gender == "true";
        //        doctors = doctors.Where(d => d.Gender == (isMale ? "Male" : "Female"));
        //    }

        //    // Lọc theo chuyên khoa
        //    if (speciality != "all")
        //    {
        //        doctors = doctors.Where(d => d.SpecialtyId == speciality);
        //    }

        //    // Sắp xếp bác sĩ
        //    switch (sort)
        //    {
        //        case "star":
        //            doctors = doctors.OrderByDescending(d => d.Feedbacks.Any() ? d.Feedbacks.Average(f => f.Star ?? 0) : 0);
        //            break;
        //        case "fee":
        //            doctors = doctors.OrderBy(d => d.Price);
        //            break;
        //        case "fee-":
        //            doctors = doctors.OrderByDescending(d => d.Price);
        //            break;
        //        default:
        //            break;
        //    }

        //    // Phân trang và lấy danh sách bác sĩ
        //    var doctorList = doctors.Select(d => new DoctorViewModel
        //    {
        //        DoctorId = d.Did,
        //        Name = d.Name,
        //        DoctorImg = d.DoctorImg,
        //        Specialty = d.Specialty.SpecialtyName,
        //        Price = d.Price ?? 0,
        //        Position = d.Position,
        //        Gender = d.Gender,
        //        NumberOfFeedbacks = d.Feedbacks.Count(),
        //        Rating = d.Feedbacks.Any() ? d.Feedbacks.Average(f => f.Star ?? 0) : 0
        //    })
        //    .Skip((pageNumber - 1) * pageSize)
        //    .Take(pageSize)
        //    .ToList();

        //    // Tính tổng số bác sĩ
        //    var totalDoctors = doctors.Count();
        //    ViewBag.TotalDoctors = totalDoctors;
        //    ViewBag.TotalPages = (int)Math.Ceiling(totalDoctors / (double)pageSize);
        //    ViewBag.PageNumber = pageNumber;

        //    // Giữ lại các tham số lọc để truyền vào view
        //    ViewBag.Gender = gender;
        //    ViewBag.Speciality = speciality;
        //    ViewBag.Sort = sort;

        //    return View(doctorList);
        //}

        public IActionResult DoctorList(string query, string[] facultiesSelected, int pageNumber = 1)
        {
            var user = _userDAO.GetLoggedInUser(User) ?? new UserProfileViewModel();
            ViewBag.User = user; // Truyền thông tin người dùng vào ViewBag
            // Lấy danh sách bác sĩ từ cơ sở dữ liệu
            var doctors = dc.Doctors.Include(d => d.Specialty).Include(d => d.Feedbacks).AsQueryable();

            // Thêm logic tìm kiếm
            if (!string.IsNullOrEmpty(query))
            {
                doctors = doctors.Where(d => d.Name.Contains(query) || d.Position.Contains(query));
            }

            // Thêm logic lọc theo chuyên khoa
            if (facultiesSelected != null && facultiesSelected.Length > 0)
            {
                // Chuyển facultiesSelected thành List<string>
                var selectedSpecialties = facultiesSelected.ToList();
                doctors = doctors.Where(d => selectedSpecialties.Contains(d.SpecialtyId.ToString()));
            }

            // Đếm tổng số bác sĩ
            var totalDoctors = doctors.Count();

            // Số lượng bản ghi trên mỗi trang
            int pageSize = 10;
            var totalPages = (int)Math.Ceiling((double)totalDoctors / pageSize);

            // Lấy danh sách bác sĩ cho trang hiện tại
            var doctorList = doctors.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            // Chuyển đổi danh sách Doctor thành danh sách DoctorViewModel
            var doctorViewModels = doctorList.Select(d => new DoctorViewModel
            {
                DoctorId = d.Did,
                Name = d.Name,
                DoctorImg = d.DoctorImg,
                Specialty = d.Specialty.SpecialtyName,
                Price = d.Price ?? 0,
                Position = d.Position,
                Gender = d.Gender,
                NumberOfFeedbacks = d.Feedbacks.Count(),
                Rating = d.Feedbacks.Any() ? d.Feedbacks.Average(f => f.Star ?? 0) : 0
            }).ToList();

            // Lưu vào ViewBag để sử dụng trong view
            ViewBag.TotalDoctors = totalDoctors;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageNumber = pageNumber;
            ViewBag.Query = query;
            ViewBag.Specialties = dc.Specialties.ToList(); // Lấy danh sách chuyên khoa
            ViewBag.SpecialtySelected = facultiesSelected?.ToList() ?? new List<string>(); // Chuyên khoa được chọn
            return View(doctorViewModels); // Trả về danh sách DoctorViewModel
        }



        //--------------------------------------------------------------------------------------------

        public IActionResult DoctorDetail(string id)
        {
            var user = _userDAO.GetLoggedInUser(User) ?? new UserProfileViewModel();
            ViewBag.User = user;

            // Lấy thông tin bác sĩ cùng với các thông tin chi tiết và phản hồi
            var doctor = dc.Doctors
                .Include(d => d.Specialty)                // Bao gồm thông tin Specialty
                .Include(d => d.Feedbacks)                // Bao gồm Feedbacks
                .Include(d => d.DetailDoctors)             // Bao gồm DetailDoctor information
                .FirstOrDefault(d => d.Did == id);        // Tìm bác sĩ theo ID

            if (doctor == null)
            {
                return NotFound();
            }

            // Chuẩn bị ViewModel
            var viewModel = new DoctorViewModel
            {
                DoctorId = doctor.Did,
                Name = doctor.Name,
                DoctorImg = doctor.DoctorImg,
                Specialty = doctor.Specialty?.SpecialtyName,
                Price = doctor.Price ?? 0,
                Position = doctor.Position,
                Phone = doctor.Phone,
                Gender = doctor.Gender,
                NumberOfFeedbacks = doctor.Feedbacks.Count(),
                Rating = doctor.Feedbacks.Any() ? doctor.Feedbacks.Average(f => f.Star ?? 0) : 0,
                Description = doctor.Description,
                Feedbacks = doctor.Feedbacks.ToList(),
                DetailDoctors = doctor.DetailDoctors.Select(dd => new DetailDoctorViewModel
                {
                    DetailId = dd.DetailId,
                    Title = dd.Title,
                    Content = dd.Content
                }).ToList()
            };

            return View(viewModel);
        }


        //--------------------------------------------------------------------------------------------

        public IActionResult Login()
        {
            var isAuthenticated = User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        //[HttpPost]
        //public async Task<IActionResult> Login(string email, string password)
        //{
        //    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        //    {
        //        TempData["ErrorMessage"] = "Please enter both email and password.";
        //        return RedirectToAction("Login");
        //    }

        //    var user = dc.Accounts.Where(dc => dc.Email == email).FirstOrDefault();
        //    if (user != null && user.Password == password)
        //    {
        //        var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.Email, email),
        //    new Claim(ClaimTypes.Role, user.Role.ToString())
        //};

        //        var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
        //        var authProperties = new AuthenticationProperties
        //        {
        //            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
        //            IsPersistent = true
        //        };

        //        await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

        //        TempData["SuccessMessage"] = "Login successful!";

        //        // Điều hướng người dùng dựa trên role
        //        switch (user.Role)
        //        {
        //            case 0: // Admin
        //                return RedirectToAction("Index", "Admin");
        //            case 1: // Staff
        //                return RedirectToAction("AppoitmentList", "Staff");
        //            case 2: // Doctor
        //                return RedirectToAction("ViewAppointment", "Doctor");
        //            case 3: // Patient
        //                return RedirectToAction("Index", "Home");
        //            default:
        //                TempData["ErrorMessage"] = "Role not recognized.";
        //                return RedirectToAction("Login");
        //        }
        //    }

        //    TempData["ErrorMessage"] = "Invalid email or password.";
        //    return RedirectToAction("Login");
        //}

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return Json(new { status = false, mess = "Please enter both email and password." });
            }

            var user = dc.Accounts.FirstOrDefault(dc => dc.Email == email);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name,user.Id),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

                var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
                var authProperties = new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                    IsPersistent = true
                };

                await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

                // Trả về phản hồi thành công và điều hướng dựa trên vai trò
                string redirectUrl = user.Role switch
                {
                    0 => Url.Action("Index", "Admin"), // Admin
                    1 => Url.Action("AppoitmentList", "Staff"), // Staff
                    2 => Url.Action("Profile", "Doctor"), // Doctor
                    3 => Url.Action("Index", "Home"), // Patient
                    _ => null // Nếu role không hợp lệ
                };

                if (redirectUrl != null)
                {
                    return Json(new { status = true, mess = "Login successful!", redirectUrl });
                }
                else
                {
                    return Json(new { status = false, mess = "Role not recognized." });
                }
            }

            // Trả về thông báo nếu email hoặc mật khẩu không hợp lệ
            return Json(new { status = false, mess = "Invalid email or password." });
        }


        //--------------------------------------------------------------------------------------------

        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Home");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }
        public async Task<IActionResult> GoogleResponse()
        {
            // Try to authenticate using MyCookieAuth scheme
            var result = await HttpContext.AuthenticateAsync("MyCookieAuth");

            if (result?.Principal == null)
            {
                // Khi người dùng từ chối hoặc không có Principal
                TempData["ErrorMessage"] = "Bạn đã hủy quá trình đăng nhập hoặc không có quyền truy cập.";
                return RedirectToAction("Login");
            }

            // Tiếp tục xử lý khi đăng nhập thành công
            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });

            var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var user = dc.Accounts.Where(dc => dc.Email == username).FirstOrDefault();

            if (user == null)
            {
                return View("Login");
            }

            var addClaim = new List<Claim>()
    {

        new Claim(ClaimTypes.Email, username),
		new Claim(ClaimTypes.Name,user.Id),
		new Claim(ClaimTypes.Role, user.Role.ToString()),
    };

            var identity = new ClaimsIdentity(addClaim, "MyCookieAuth");
            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                IsPersistent = true
            };

            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("MyCookieAuth", principal, authProperties);

            // Điều hướng theo vai trò
            switch (user.Role)
            {
                case 0: // Admin
                    return RedirectToAction("Index", "Admin");
                case 1: // Staff
                    return RedirectToAction("AppoitmentList", "Staff");
                case 2: // Doctor
                    return RedirectToAction("ViewAppointment", "Doctor");
                case 3: // Patient
                    return RedirectToAction("Index", "Home");
                default:
                    TempData["ErrorMessage"] = "Role không được nhận diện.";
                    return RedirectToAction("Login");
            }
        }



        public IActionResult SignUp()
        {
            return View();
        }


        //public IActionResult SignUp1(RegisterViewModel model)
        //{
        //    // Kiểm tra tính hợp lệ của model
        //    if (!ModelState.IsValid)
        //    {
        //        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        //        return Json(new { status = false, mess = "Có lỗi trong dữ liệu nhập: " + string.Join(", ", errors) });
        //    }

        //    // Kiểm tra mật khẩu
        //    if (model.Password.Length < 8 || !Regex.IsMatch(model.Password, @"[A-Za-z]") || !Regex.IsMatch(model.Password, @"[0-9]"))
        //    {
        //        ModelState.AddModelError(nameof(model.Password), "Mật khẩu phải có ít nhất 8 ký tự, bao gồm cả chữ cái và số.");
        //        return Json(new { status = false, mess = "Mật khẩu không hợp lệ." });
        //    }

        //    // Kiểm tra xem tài khoản đã tồn tại chưa
        //    var existingAccount = dc.Accounts.FirstOrDefault(a => a.Username == model.Username || a.Email == model.Email);
        //    if (existingAccount != null)
        //    {
        //        ModelState.AddModelError("", "Tài khoản hoặc email đã tồn tại.");
        //        return Json(new { status = false, mess = "Tài khoản hoặc email đã tồn tại." });
        //    }

        //    try
        //    {
        //        // Tạo tài khoản mới
        //        var newAccount = new Account
        //        {
        //            Id = Guid.NewGuid().ToString(),
        //            Username = model.Username,
        //            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
        //            Email = model.Email,
        //            Role = 3, // Mặc định là Patient
        //            Status = true
        //        };

        //        // Thêm tài khoản vào cơ sở dữ liệu
        //        dc.Accounts.Add(newAccount);
        //        dc.SaveChanges();

        //        // Tạo bản ghi Patient
        //        var newPatient = new Patient
        //        {
        //            Pid = newAccount.Id,
        //            Name = model.FullName,
        //            Phone = model.Phone,
        //            Gender = model.Gender ? "Nam" : "Nữ",
        //            Dob = DateOnly.FromDateTime(model.DateOfBirth)
        //        };

        //        // Thêm patient vào cơ sở dữ liệu
        //        dc.Patients.Add(newPatient);
        //        dc.SaveChanges();

        //        return Json(new { status = true, mess = "Đăng ký thành công! Chuyển đến trang đăng nhập." });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Ghi log lỗi ra console hoặc file log (nếu có)
        //        Console.WriteLine("Có lỗi xảy ra: " + ex.ToString());
        //        ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
        //        return Json(new { status = false, mess = "Có lỗi xảy ra: " + ex.Message });
        //    }

        //}

        public IActionResult SignUp1(RegisterViewModel model)
        {
            // Kiểm tra tính hợp lệ của model
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { status = false, mess = "Có lỗi trong dữ liệu nhập: " + string.Join(", ", errors) });
            }

            // Kiểm tra mật khẩu
            if (model.Password.Length < 8 || !Regex.IsMatch(model.Password, @"[A-Za-z]") || !Regex.IsMatch(model.Password, @"[0-9]"))
            {
                ModelState.AddModelError(nameof(model.Password), "Mật khẩu phải có ít nhất 8 ký tự, bao gồm cả chữ cái và số.");
                return Json(new { status = false, mess = "Mật khẩu không hợp lệ." });
            }

            // Kiểm tra xác nhận mật khẩu
            if (model.Password != model.RePassword)
            {
                ModelState.AddModelError(nameof(model.RePassword), "Xác nhận mật khẩu không khớp.");
                return Json(new { status = false, mess = "Xác nhận mật khẩu không khớp." });
            }

            // Kiểm tra ngày sinh
            if (model.DateOfBirth > DateTime.Now)
            {
                ModelState.AddModelError(nameof(model.DateOfBirth), "Ngày sinh không được vượt quá hôm nay.");
                return Json(new { status = false, mess = "Ngày sinh không hợp lệ." });
            }

            // Kiểm tra xem tài khoản đã tồn tại chưa
            var existingAccount = dc.Accounts.FirstOrDefault(a => a.Username == model.Username || a.Email == model.Email);
            if (existingAccount != null)
            {
                ModelState.AddModelError("", "Tài khoản hoặc email đã tồn tại.");
                return Json(new { status = false, mess = "Tài khoản hoặc email đã tồn tại." });
            }

            try
            {
                // Tạo tài khoản mới
                var newAccount = new Account
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = model.Username,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Email = model.Email,
                    Role = 3, // Mặc định là Patient
                    Status = true
                };

                // Thêm tài khoản vào cơ sở dữ liệu
                dc.Accounts.Add(newAccount);
                dc.SaveChanges();

                // Tạo bản ghi Patient
                var newPatient = new Patient
                {
                    Pid = newAccount.Id,
                    Name = model.FullName,
                    Phone = model.Phone,
                    Gender = model.Gender ? "Nam" : "Nữ",
                    Dob = DateOnly.FromDateTime(model.DateOfBirth)
                };

                // Thêm patient vào cơ sở dữ liệu
                dc.Patients.Add(newPatient);
                dc.SaveChanges();


				// Gửi email thông báo tạo tài khoản
                var subject = "Tạo tài khoản thành công";
				var body = $"Xin chào {model.FullName},<br/><br/>" +
						   "Tài khoản của bạn đã được tạo thành công tại DocCare.<br/><br/>" +
						   "Trân trọng,<br/>" +
						   "Đội ngũ DocCare";

				// Gửi email thông báo tạo tài khoản
				Task.Run(() => _emailService.SendEmailAsync(model.Email, subject, body));

				return Json(new { status = true, mess = "Đăng ký thành công! Chuyển đến trang đăng nhập.", redirectUrl = Url.Action("Login", "Home") });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ra console hoặc file log (nếu có)
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return Json(new { status = false, mess = "Có lỗi xảy ra: " + ex.Message });
            }
        }


        //--------------------------------------------------------------------------------------------


        //--------------------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            // Trả về view ForgotPassword với một model mới
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            // Kiểm tra nếu dữ liệu của model không hợp lệ
            if (!ModelState.IsValid)
            {
                model.Message = "Please fill out the required fields correctly.";
                // Nếu model không hợp lệ, trả về view với các thông báo lỗi
                return View(model);
            }

            // Tìm kiếm account dựa trên email mà người dùng nhập vào
            var account = await dc.Accounts.FirstOrDefaultAsync(a => a.Email == model.Email);

            // Nếu không tìm thấy account tương ứng với email đã nhập
            if (account == null)
            {
                // Thêm thông báo lỗi vào ModelState để hiển thị cho người dùng
                ModelState.AddModelError(string.Empty, "The email address you entered is not associated with any account.");
                // Trả về view cùng với thông báo lỗi
                return View(model);
            }

            // Nếu email hợp lệ, tạo token để reset password và lưu vào cache
            var token = _tokenService.GenerateToken(account.Email);

            // Xây dựng liên kết để người dùng click vào và reset password
            var resetLink = Url.Action("ResetPassword", "Home", new { token = token }, Request.Scheme);

            // Cấu hình tiêu đề và nội dung email
            var subject = "Reset your password";
            var body = $"Please click the link below to reset your password: <a href='{resetLink}'>Reset Password</a>. This link will expired in 15 minutes";

            // Gửi email cho người dùng với liên kết reset password
            await _emailService.SendEmailAsync(account.Email, subject, body);

            // Gán thông báo thành công vào model để hiển thị trên view
            model.Message = "A reset link has been sent to your email.";
            model.IsSuccess = true;
            TempData["SuccessMessage"] = model.Message;

            // Trả về view với model đã cập nhật thông báo thành công
            return View(model);
        }

        //ResetPassword

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            // Kiểm tra tính hợp lệ của token và lấy email nếu hợp lệ
            if (_tokenService.ValidateToken(token, out string email))
            {
                // Tạo model cho reset password với token đã xác thực
                var model = new ResetPasswordViewModel
                {
                    Token = token,
                    Email = email
                };
                // Trả về view reset password cùng với model đã khởi tạo
                return View(model);
            }

            // Nếu token không hợp lệ hoặc đã hết hạn, hiển thị thông báo lỗi
            ViewBag.Message = "Invalid or expired token. PLease back to Forgot Password Page and send email again!";
            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            // Kiểm tra nếu dữ liệu model không hợp lệ
            if (!ModelState.IsValid)
            {
                // Trả về view với các thông báo lỗi nếu có
                return View(model);
            }

            //// Kiểm tra token để xác thực và lấy email nếu token hợp lệ
            //if (!_tokenService.ValidateToken(model.Token, out string email))
            //{
            //    // Thêm lỗi vào ModelState khi token không hợp lệ hoặc đã hết hạn
            //    ModelState.AddModelError("", "Invalid or expired token.");
            //    return View(model);
            //}

            // Tìm kiếm account dựa trên email đã xác thực từ token
            var account = await dc.Accounts.FirstOrDefaultAsync(a => a.Email == model.Email);

            // Nếu không tìm thấy account tương ứng
            if (account == null)
            {
                // Thêm lỗi vào ModelState khi không tìm thấy account
                ModelState.AddModelError("", "Account not found.");
                return View(model);
            }

            // Cập nhật mật khẩu mới cho account (không mã hóa vì bạn đang học)
            account.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            // Cập nhật thông tin account vào database
            dc.Update(account);
            await dc.SaveChangesAsync();

            // Thêm thông báo thành công vào TempData để hiển thị sau khi chuyển hướng
            TempData["SuccessMessage"] = "Your password has been reset successfully.";
            return RedirectToAction("Login");
        }
        //--------------------------------------------------------------------------------------------

        public IActionResult ServiceList(int pageNumber = 1, string search = "", string sort = "")
        {
            var user = _userDAO.GetLoggedInUser(User) ?? new UserProfileViewModel();
            ViewBag.User = user; // Truyền thông tin người dùng vào ViewBag
            int pageSize = 6;

            // Lấy danh sách dịch vụ
            var services = dc.Specialties.AsQueryable();

            // Thực hiện tìm kiếm nếu có
            if (!string.IsNullOrWhiteSpace(search))
            {
                services = services.Where(s => s.SpecialtyName.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(sort))
            {
                switch (sort)
                {
                    case "1":
                        services = services.OrderBy(s => s.SpecialtyName);
                        break;
                    case "2":
                        services = services.OrderByDescending(s => s.SpecialtyName);
                        break;
                }
            }

            var totalServices = services.Count();

            var pagedServices = services
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            ViewBag.TotalServices = totalServices;
            ViewBag.TotalPages = (int)Math.Ceiling(totalServices / (double)pageSize);
            ViewBag.PageNumber = pageNumber;
            ViewBag.Search = search;
            ViewBag.Sort = sort;

            return View(pagedServices);
        }



        //--------------------------------------------------------------------------------------------

        public IActionResult ServiceDetail(string id)
        {
            var user = _userDAO.GetLoggedInUser(User) ?? new UserProfileViewModel();
            ViewBag.User = user; // Truyền thông tin người dùng vào ViewBag

            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("ServiceList");
            }

            // Lấy specialty kèm theo các doctor và detail specialty
            var service = dc.Specialties
                .Include(s => s.Doctors)  // Lấy danh sách bác sĩ liên quan
                .Include(s => s.DetailSpecialties)  // Lấy các chi tiết specialty
                .FirstOrDefault(s => s.SpecialtyId == id);

            if (service == null)
            {
                return NotFound();
            }

            var feedbacks = dc.Feedbacks
                .Include(f => f.PidNavigation) // Bao gồm thông tin bệnh nhân
                .AsEnumerable() // This forces the query to be executed in memory.
                .Where(f => service.Doctors.Any(d => d.Did == f.Did))
                .ToList();

            // Tính toán đánh giá trung bình và tổng số đánh giá
            double averageRating = feedbacks.Any() ? feedbacks.Average(f => f.Star ?? 0) : 0;
            int totalReviews = feedbacks.Count();

            // Tạo ViewModel cho ServiceDetail
            var viewModel = new ServiceViewModel
            {
                SpecialtyId = service.SpecialtyId,
                SpecialtyName = service.SpecialtyName,
                SpecialtyImg = service.SpecialtyImg,
                ShortDescription = service.ShortDescription,
                DetailSpecialties = service.DetailSpecialties.ToList(), // Lấy chi tiết specialty
                Doctors = service.Doctors.ToList(),
                Feedbacks = feedbacks,
                AverageRating = averageRating,
                TotalReviews = totalReviews
            };

            return View(viewModel);
        }


        //--------------------------------------------------------------------------------------------



        public IActionResult Contact(ContactModel model)
        {
            if (ModelState.IsValid)
            {
                var contact = new Contact
                {
                    ContactId = Guid.NewGuid().ToString(),
                    Name = model.Name,
                    Email = model.Email,
                    Title = model.Title,
                    Description = model.Description,
                    Status = "Pending" // Trạng thái mặc định
                };

                dc.Contacts.Add(contact); // Lưu vào cơ sở dữ liệu
                dc.SaveChanges(); // Thực hiện lưu thay đổi vào cơ sở dữ liệu

                ViewBag.Message = "Tin nhắn của bạn đã được gửi thành công.";
                return View();
            }

            ViewBag.ErrorMessage = "Vui lòng điền đầy đủ thông tin để gửi liên lạc.";
            return View(model);
        }

        // If validation fails, return to the same view with error messages

    

        //--------------------------------------------------------------------------------------------

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Index", "Home");
        }


		public IActionResult aichatbox()
		{
			var isAuthenticated = User.Identity.IsAuthenticated;
			if (isAuthenticated)
			{
				var user = _userDAO.GetLoggedInUser(User) ?? new UserProfileViewModel();
				ViewBag.User = user;
			}

			return View();
		}

		[HttpPost]
		public async Task<JsonResult> getmessage(string message)
		{
			string result = await AIControl.CreateMessage(message);
			string pattern = @"\bs\d+\b";

			// Tìm tất cả các chuỗi khớp với biểu thức chính quy
			MatchCollection matches = Regex.Matches(result, pattern);

			List<Specialty> list = new List<Specialty>();
			foreach (Match match in matches)
			{
				Specialty specialty = dc.Specialties.FirstOrDefault(s => s.SpecialtyId == match.Value);
				if (specialty != null)
				{
					list.Add(specialty);
				}
			}
			result = Regex.Replace(result, pattern, "");

			// Loại bỏ khoảng trắng thừa sau khi xóa
			result = Regex.Replace(result, @"\s+", " ").Trim();
			return Json(new { content = result, specialties = list });
		}



		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



    }
}
