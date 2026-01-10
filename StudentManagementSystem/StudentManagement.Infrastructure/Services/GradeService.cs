using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.DTOs;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Interfaces;
using StudentManagement.Infrastructure.Data;

namespace StudentManagement.Infrastructure.Services
{
    public class GradeService : IGradeService
    {
        private readonly AppDbContext _context;

        public GradeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GradeResponse> UpdateGradeAsync(UpdateGradeRequest request)
        {
            // 1. Tìm Enrollment
            var enrollment = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Class).ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentId == request.EnrollmentId);

            if (enrollment == null)
                throw new Exception("Không tìm thấy bản ghi đăng ký học này.");

            // 2. Cập nhật điểm thành phần
            enrollment.MidtermScore = request.MidtermScore;
            enrollment.FinalScore = request.FinalScore;

            // 3. Tính điểm tổng kết môn (40% - 60%)
            // Làm tròn 1 chữ số thập phân (VD: 8.56 -> 8.6)
            double total = (request.MidtermScore * 0.4) + (request.FinalScore * 0.6);
            // TRƯỚC: Làm tròn 1 chữ số
            // enrollment.TotalScore = Math.Round(total, 1);

            // SAU: Làm tròn 3 chữ số thập phân (Hệ số 0.000)
            enrollment.TotalScore = Math.Round(total, 3);


            // 4. Quy đổi Grade & Trạng thái Pass/Fail
            AssignGrade(enrollment);

            // 5. Lưu tạm điểm môn này vào DB trước
            await _context.SaveChangesAsync();

            // 6. TÍNH LẠI GPA TÍCH LŨY (OVERALL GPA)
            // Lấy tất cả môn đã có điểm của SV này
            var allGrades = await _context.Enrollments
                .Include(e => e.Class).ThenInclude(c => c.Course)
                .Where(e => e.StudentId == enrollment.StudentId && e.TotalScore != null)
                .ToListAsync();

            // Lọc lấy điểm mới nhất của từng môn (Nếu học lại thì lấy điểm mới nhất)
            var latestGrades = allGrades
                .GroupBy(e => e.Class.CourseId)
                .Select(g => g.OrderByDescending(e => e.EnrollmentDate).First())
                .ToList();

            double totalPoints = latestGrades.Sum(e => (e.TotalScore ?? 0) * e.Class.Course.Credits);
            int totalCredits = latestGrades.Sum(e => e.Class.Course.Credits);

            // TRƯỚC:
            // double overallGpa = totalCredits > 0 ? Math.Round(totalPoints / totalCredits, 2) : 0;

            // SAU:
            double overallGpa = totalCredits > 0 ? Math.Round(totalPoints / totalCredits, 3) : 0;


            // Cập nhật vào bảng Student
            enrollment.Student.OverallGPA = overallGpa;
            await _context.SaveChangesAsync();

            // 7. Trả về kết quả
            return new GradeResponse
            {
                EnrollmentId = enrollment.EnrollmentId,
                StudentName = enrollment.Student.FullName,
                CourseName = enrollment.Class.Course.CourseName,
                MidtermScore = enrollment.MidtermScore.Value,
                FinalScore = enrollment.FinalScore.Value,
                TotalScore = enrollment.TotalScore.Value,
                Grade = enrollment.Grade,
                IsPassed = enrollment.IsPassed,
                NewOverallGPA = overallGpa
            };
        }

        // Hàm phụ trợ: Quy đổi điểm số ra chữ cái
        private void AssignGrade(Enrollment e)
        {
            double score = e.TotalScore ?? 0;

            if (score >= 8.5) e.Grade = "A";
            else if (score >= 7.0) e.Grade = "B";
            else if (score >= 5.5) e.Grade = "C";
            else if (score >= 4.0) e.Grade = "D";
            else e.Grade = "F";

            e.IsPassed = (e.Grade != "F");
        }
    }
}
