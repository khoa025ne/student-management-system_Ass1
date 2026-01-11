using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.DTOs;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Interfaces;
using StudentManagement.Infrastructure.Data;
using StudentManagement.Core.Entities;


namespace StudentManagement.Infrastructure.Services
{
    public class GradeService : IGradeService
    {
        private readonly AppDbContext _context;
        private readonly IAiAnalysisService _aiAnalysisService; // service AI mới

        public GradeService(AppDbContext context, IAiAnalysisService aiAnalysisService)
        {
            _context = context;
            _aiAnalysisService = aiAnalysisService;
        }

        public async Task<GradeResponse> UpdateGradeAsync(UpdateGradeRequest request)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Class).ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentId == request.EnrollmentId);

            if (enrollment == null)
                throw new Exception("Không tìm thấy bản ghi đăng ký học này.");

            if (request.MidtermScore is < 0 or > 10 ||
                request.FinalScore is < 0 or > 10)
                throw new Exception("Điểm phải nằm trong khoảng 0 - 10.");

            enrollment.MidtermScore = request.MidtermScore;
            enrollment.FinalScore = request.FinalScore;

            double total = (request.MidtermScore * 0.4) + (request.FinalScore * 0.6);
            enrollment.TotalScore = Math.Round(total, 3);

            AssignGrade(enrollment);
            enrollment.Status = "Completed";

            await _context.SaveChangesAsync();

            var allGrades = await _context.Enrollments
                .Include(e => e.Class).ThenInclude(c => c.Course)
                .Where(e => e.StudentId == enrollment.StudentId && e.TotalScore != null)
                .ToListAsync();

            var latestGrades = allGrades
                .GroupBy(e => e.Class.CourseId)
                .Select(g => g.OrderByDescending(e => e.EnrollmentDate).First())
                .ToList();

            double totalPoints = latestGrades.Sum(e => (e.TotalScore ?? 0) * e.Class.Course.Credits);
            int totalCredits = latestGrades.Sum(e => e.Class.Course.Credits);

            double overallGpa = totalCredits > 0
                ? Math.Round(totalPoints / totalCredits, 3)
                : 0;

            enrollment.Student.OverallGPA = overallGpa;
            await _context.SaveChangesAsync();

            // Trigger notification
            var noti = new Notification
            {
                StudentId = enrollment.StudentId,
                Title = "Điểm môn học mới",
                Message = $"Điểm môn {enrollment.Class.Course.CourseName}: " +
                          $"{enrollment.TotalScore:0.0} ({enrollment.Grade}). " +
                          $"GPA tích lũy hiện tại: {overallGpa:0.00}",
                Type = "ScoreUpdate"
            };

            _context.Notifications.Add(noti);
            await _context.SaveChangesAsync();

            // AI phân tích nền
            _ = _aiAnalysisService.GenerateAndSaveAnalysisAsync(enrollment.StudentId);

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

            e.IsPassed = e.Grade != "F";
        }
    }
}
