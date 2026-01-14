using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Core.DTOs
{
    public class UpdateGradeRequest
    {
        [Required]
        public int EnrollmentId { get; set; } // ID của lần đăng ký học

        [Range(0, 10, ErrorMessage = "Điểm Midterm phải từ 0 đến 10")]
        public double MidtermScore { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm Final phải từ 0 đến 10")]
        public double FinalScore { get; set; }
    }

    public class GradeResponse
    {
        public int EnrollmentId { get; set; }
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public double MidtermScore { get; set; }
        public double FinalScore { get; set; }
        public double TotalScore { get; set; } // Điểm tổng kết môn
        public string Grade { get; set; }      // A, B, C...
        public bool IsPassed { get; set; }
        public double NewOverallGPA { get; set; } // GPA tích lũy mới nhất của SV
    }
}

