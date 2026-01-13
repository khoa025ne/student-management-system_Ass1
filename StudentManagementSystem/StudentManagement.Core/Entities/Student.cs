using System;
using System.Collections.Generic;

namespace StudentManagement.Core.Entities
{
    public class Student
    {
        public int StudentId { get; set; }

        public string StudentCode { get; set; } // STU202300145
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }

        public string ClassCode { get; set; } // SE1801
        public double OverallGPA { get; set; }
        public DateTime CreatedAt { get; set; }

        public string Major { get; set; } // "SE", "IA", ...

        public string? AvatarUrl { get; set; } // "/uploads/sv123.jpg"

        // MỚI: dùng cho Flow 3 (khuyến nghị theo kỳ)
        public int? CurrentTermNo { get; set; }

        // MỚI: dùng cho Flow 1 (đăng nhập lần đầu)
        public bool IsFirstLogin { get; set; } = true;

        // Navigation Properties
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<Score> Scores { get; set; }

        public ICollection<AcademicAnalysis> Analyses { get; set; }
    }
}
