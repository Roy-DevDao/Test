using test2.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace test2.DAO
{
    public class FeedbackDAO
    {
        private readonly DocCareContext dc;

        public FeedbackDAO(DocCareContext context)
        {
            dc = context;
        }

        // Phương thức lấy các phản hồi của bác sĩ theo Did
        public List<Feedback> GetFeedbacksByDoctorId(string did)
        {
            return dc.Feedbacks
                .Include(f => f.DidNavigation) // Bao gồm thông tin bác sĩ
                .Include(f => f.PidNavigation) // Bao gồm thông tin bệnh nhân
                .Where(f => f.Did == did)
                .ToList();
        }
    }
}
