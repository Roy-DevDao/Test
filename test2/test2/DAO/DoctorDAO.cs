using test2.Data;

namespace test2.DAO
{
    public class DoctorDAO
    {
        private readonly DocCareContext dc;

        public DoctorDAO(DocCareContext context)
        {
            dc = context;
        }

        // Lấy danh sách bác sĩ
        public List<Doctor> GetAllDoctors()
        {
            return dc.Doctors.ToList();
        }

        // Lấy thông tin chi tiết bác sĩ theo ID
        public Doctor? GetDoctorById(string did)
        {
            return dc.Doctors.FirstOrDefault(d => d.Did == did);
        }
    }
}
