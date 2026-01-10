using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace StudentManagement.Core.Entities
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;
        public string Status { get; set; } // Active, Cancelled, Failed
        //bỏ ispassed, sửa các trạng thái của status là (pass, not pass, pending, not started )

        // Navigation
        public Student Student { get; set; }
        public Class Class { get; set; }

        // ĐIỂM SỐ
        public double? MidtermScore { get; set; } // Điểm giữa kỳ
        public double? FinalScore { get; set; }   // Điểm cuối kỳ
        public double? TotalScore { get; set; }   // Tổng kết (VD: 40% Mid + 60% Final)

        public string? Grade { get; set; }  // A, B, C, D, F (Tính tự động)
        public bool IsPassed { get; set; } // True nếu qua môn

        // VẤN ĐỀ 3: ĐẾM SỐ LẦN HỌC
        // Khi đăng ký, ta sẽ query xem SV này đã học môn này bao nhiêu lần rồi + 1
        public int AttemptNumber { get; set; } = 1; // Lần học thứ mấy (1, 2, 3...)
    }
}
