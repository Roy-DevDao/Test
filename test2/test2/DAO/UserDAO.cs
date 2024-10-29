using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using test2.Data;
using test2.Models;

namespace test2.DAO
{
    public class UserDAO
    {
        private readonly DocCareContext _context;

        public UserDAO(DocCareContext dc)
        {
            _context = dc;
        }

		//public UserProfileViewModel GetLoggedInUser(ClaimsPrincipal user)
		//{
		//    var email = user.FindFirst(ClaimTypes.Email)?.Value;

		//    if (email == null)
		//    {
		//        return null;
		//    }

		//    // Lấy thông tin tài khoản
		//    var account = _context.Accounts.FirstOrDefault(a => a.Email == email);
		//    if (account == null)
		//    {
		//        return null;
		//    }

		//    // Tạo đối tượng UserProfileViewModel
		//    var userProfile = new UserProfileViewModel
		//    {
		//        Id = account.Id,
		//        Username = account.Username,
		//        Email = account.Email,
		//        Password = account.Password, // Staff và Admin cần Password
		//        Role = account.Role,
		//        Status = account.Status
		//    };
		//    // Kiểm tra role của user và gán thông tin tương ứng
		//    switch (account.Role)
		//    {
		//        case 3: // Patient
		//            var patient = _context.Patients.FirstOrDefault(p => p.Pid == account.Id);
		//            if (patient != null)
		//            {
		//                userProfile.PatientName = patient.Name;
		//                userProfile.PatientImg = patient.PatientImg;
		//                userProfile.PPhone = patient.Phone;
		//                userProfile.PGender = patient.Gender;
		//                userProfile.PDob = patient.Dob;
		//            }
		//            break;

		//        case 2: // Doctor
		//            var doctor = _context.Doctors.FirstOrDefault(d => d.Did == account.Id);
		//            if (doctor != null)
		//            {
		//                userProfile.DoctorName = doctor.Name;
		//                userProfile.DoctorImg = doctor.DoctorImg;
		//                userProfile.Position = doctor.Position;
		//                userProfile.DPhone = doctor.Phone;
		//                userProfile.DGender = doctor.Gender;
		//                userProfile.DDob = doctor.Dob;
		//                userProfile.Description = doctor.Description;
		//                userProfile.Price = doctor.Price;
		//                userProfile.SpecialtyId = doctor.SpecialtyId;
		//            }
		//            break;

		//        case 1: // Staff
		//            var staff = _context.Staff.FirstOrDefault(s => s.StaffId == account.Id);
		//            if (staff != null)
		//            {
		//                userProfile.StaffName = staff.Name;
		//                userProfile.StaffImg = staff.StaffImg;
		//                userProfile.SPhone = staff.Phone;
		//                userProfile.SGender = staff.Gender;
		//                userProfile.SDob = staff.Dob;
		//            }
		//            break;
		//        default:
		//            return null;
		//    }

		//    return userProfile;
		//}

		public UserProfileViewModel GetLoggedInUser(ClaimsPrincipal user)
		{
			var email = user.FindFirst(ClaimTypes.Email)?.Value;

			if (email == null)
			{
				return null;
			}

			// Lấy thông tin tài khoản
			var account = _context.Accounts.FirstOrDefault(a => a.Email == email);
			if (account == null)
			{
				return null;
			}

			// Tạo đối tượng UserProfileViewModel
			var userProfile = new UserProfileViewModel
			{
				Id = account.Id,
				Username = account.Username,
				Email = account.Email,
				Password = account.Password, // Staff và Admin cần Password
				Role = account.Role,
				Status = account.Status
			};

			// Kiểm tra role của user và gán thông tin tương ứng
			switch (account.Role)
			{
				case 3: // Patient
					var patient = _context.Patients.FirstOrDefault(p => p.Pid == account.Id);
					if (patient != null)
					{
						userProfile.PatientName = patient.Name;
						userProfile.PatientImg = patient.PatientImg;
						userProfile.PPhone = patient.Phone;
						userProfile.PGender = patient.Gender;
						userProfile.PDob = patient.Dob;
					}
					break;

				case 2: // Doctor
					var doctor = _context.Doctors.FirstOrDefault(d => d.Did == account.Id);
					if (doctor != null)
					{
						userProfile.DoctorName = doctor.Name;
						userProfile.DoctorImg = doctor.DoctorImg;
						userProfile.Position = doctor.Position;
						userProfile.DPhone = doctor.Phone;
						userProfile.DGender = doctor.Gender;
						userProfile.DDob = doctor.Dob;
						userProfile.Description = doctor.Description;
						userProfile.Price = doctor.Price;
						userProfile.SpecialtyId = doctor.SpecialtyId;
					}
					break;

				case 1: // Staff
					var staff = _context.Staff.FirstOrDefault(s => s.StaffId == account.Id);
					if (staff != null)
					{
						userProfile.StaffName = staff.Name;
						userProfile.StaffImg = staff.StaffImg;
						userProfile.SPhone = staff.Phone;
						userProfile.SGender = staff.Gender;
						userProfile.SDob = staff.Dob;
					}
					break;
				default:
					return null;
			}

			return userProfile;
		}

	}
}