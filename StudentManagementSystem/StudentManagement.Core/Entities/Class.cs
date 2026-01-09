using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace StudentManagement.Core.Entities
{
    public class Class
    {
        public int ClassId { get; set; }

        // Tên lớp: SE1801, AI1905...
        public string ClassName { get; set; }

        // Sĩ số
        public int MaxCapacity { get; set; } = 30; // Mặc định 30
        public int CurrentEnrollment { get; set; } = 0;

        // Lịch học (Giản lược)
        public string Room { get; set; } // P.304
        public string Schedule { get; set; } // Mon-Wed-Fri (Ca 1)

        // LIÊN KẾT QUAN TRỌNG:

        // 1. Thuộc về Môn nào? (Ví dụ: Lớp SE1801 dạy môn PRN211)
        public int CourseId { get; set; }
        public Course Course { get; set; }

        // 2. Thuộc về Học kỳ nào? (Ví dụ: Spring 2025)
        public int SemesterId { get; set; }
        public Semester Semester { get; set; }

        // 3. Ai dạy? (Tạm thời để null nếu chưa làm Teacher)
        public int? TeacherId { get; set; }
        // public User Teacher { get; set; }

        // Danh sách sinh viên đăng ký vào lớp này
        // public ICollection<Enrollment> Enrollments { get; set; }
    }
}
