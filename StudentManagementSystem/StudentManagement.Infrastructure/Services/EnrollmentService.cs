using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Infrastructure.Data;

namespace StudentManagement.Infrastructure.Services
{
    public class EnrollmentService
    {
        private readonly AppDbContext _context;

        public EnrollmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> RegisterCourseAsync(int studentId, int classId)
        {
            // 1. Lấy thông tin Lớp đang muốn đăng ký
            var classInfo = await _context.Classes
                .Include(c => c.Course) // Load thông tin môn học
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            if (classInfo == null) return "Lớp học không tồn tại.";

            // 2. Check Sĩ số
            if (classInfo.CurrentEnrollment >= classInfo.MaxCapacity)
                return "Lớp đã đầy (Full Capacity).";

            // 3. Check Trùng lịch
            bool isScheduleConflict = await _context.Enrollments
                .Include(e => e.Class)
                .AnyAsync(e => e.StudentId == studentId &&
                               e.Class.SemesterId == classInfo.SemesterId &&
                               e.Class.Schedule == classInfo.Schedule &&
                               e.Status == "Active");

            if (isScheduleConflict) return "Bạn bị trùng lịch học với môn khác!";

            // 4. Check Điều kiện tiên quyết (Prerequisite)
            if (classInfo.Course.PrerequisiteCourseId != null)
            {
                // 4a. Lấy tên môn tiên quyết để báo lỗi cho rõ
                var preCourse = await _context.Courses
                    .FindAsync(classInfo.Course.PrerequisiteCourseId);

                string preCourseName = preCourse?.CourseCode ?? "Unknown"; // Ví dụ: PRF192

                // 4b. Kiểm tra lịch sử học
                var prerequisiteHistory = await _context.Enrollments
                    .Include(e => e.Class)
                    .Where(e => e.StudentId == studentId &&
                                e.Class.CourseId == classInfo.Course.PrerequisiteCourseId)
                    .OrderByDescending(e => e.EnrollmentDate)
                    .FirstOrDefaultAsync();

                // Điều kiện: Chưa từng học HOẶC (Đã học nhưng rớt/chưa qua)
                if (prerequisiteHistory == null || !prerequisiteHistory.IsPassed)
                {
                    return $"Bạn chưa qua môn tiên quyết: {preCourseName} (Prerequisite not passed).";
                }
            }

            // 5. Tính số lần học (Attempt Number)
            int previousAttempts = await _context.Enrollments
                .Include(e => e.Class)
                .CountAsync(e => e.StudentId == studentId && e.Class.CourseId == classInfo.CourseId);

            // 6. Tạo Enrollment mới
            var enrollment = new Enrollment
            {
                StudentId = studentId,
                ClassId = classId,
                Status = "Active",
                EnrollmentDate = DateTime.Now,
                AttemptNumber = previousAttempts + 1, // Lần học thứ mấy
                IsPassed = false // Mới đăng ký thì chưa đậu
            };

            // 7. Cập nhật sĩ số lớp
            classInfo.CurrentEnrollment++;

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return "Success";
        }
    }
}
