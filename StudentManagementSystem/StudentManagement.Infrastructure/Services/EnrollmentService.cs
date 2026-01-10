using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.Infrastructure.Services
{
    public class EnrollmentService
    {
        private readonly AppDbContext _context;

        public EnrollmentService(AppDbContext context)
        {
            _context = context;
        }

        // ĐĂNG KÝ MÔN HỌC
        public async Task<string> RegisterCourseAsync(int studentId, int classId)
        {
            // 1. Tìm lớp
            var cls = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            if (cls == null)
                return "Class not found";

            // 2. Check sĩ số
            if (cls.CurrentEnrollment >= cls.MaxCapacity)
                return "Class is full";

            // 3. Check tiên quyết (nếu môn có Prerequisite)
            if (cls.Course.PrerequisiteCourseId.HasValue)
            {
                var preId = cls.Course.PrerequisiteCourseId.Value;

                bool hasPassedPrerequisite = await _context.Enrollments
                    .AnyAsync(e =>
                        e.StudentId == studentId &&
                        e.Class.CourseId == preId &&
                        e.IsPassed);

                if (!hasPassedPrerequisite)
                    return "Chưa đạt môn tiên quyết";
            }

            // 4. Check trùng lịch: cùng kỳ + cùng cặp ngày + cùng slot
            var existingEnrollments = await _context.Enrollments
                .Include(e => e.Class)
                .Where(e => e.StudentId == studentId &&
                            e.Status == "Active" &&
                            e.Class.SemesterId == cls.SemesterId)
                .ToListAsync();

            bool hasConflict = existingEnrollments.Any(e =>
                e.Class.DayOfWeekPair == cls.DayOfWeekPair &&
                e.Class.TimeSlot == cls.TimeSlot
            );

            if (hasConflict)
                return "Lịch học trùng với một lớp khác trong cùng Slot/Cặp ngày";

            // 5. Tạo enrollment
            var enrollment = new Enrollment
            {
                StudentId = studentId,
                ClassId = classId,
                Status = "Active",
                EnrollmentDate = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);
            cls.CurrentEnrollment += 1;

            await _context.SaveChangesAsync();
            return "Success";
        }

        // HÀM ĐỔI LỚP (đã thêm trước đó)
        public async Task<string> ChangeClassAsync(int studentId, int oldClassId, int newClassId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId &&
                                          e.ClassId == oldClassId &&
                                          e.Status == "Active");

            if (enrollment == null)
                return "Enrollment not found";

            var oldClass = await _context.Classes.FindAsync(oldClassId);
            var newClass = await _context.Classes.FindAsync(newClassId);

            if (newClass == null)
                return "New class not found";

            if (oldClass.CourseId != newClass.CourseId)
                return "Chỉ được đổi giữa các lớp của cùng một môn";

            if (newClass.CurrentEnrollment >= newClass.MaxCapacity)
                return "New class is full";

            var otherEnrollments = await _context.Enrollments
                .Include(e => e.Class)
                .Where(e => e.StudentId == studentId &&
                            e.ClassId != oldClassId &&
                            e.Status == "Active" &&
                            e.Class.SemesterId == newClass.SemesterId)
                .ToListAsync();

            bool conflict = otherEnrollments.Any(e =>
                e.Class.DayOfWeekPair == newClass.DayOfWeekPair &&
                e.Class.TimeSlot == newClass.TimeSlot
            );

            if (conflict)
                return "Lịch new class trùng với một lớp khác";

            enrollment.ClassId = newClassId;

            oldClass.CurrentEnrollment -= 1;
            newClass.CurrentEnrollment += 1;

            await _context.SaveChangesAsync();
            return "Success";
        }
    }
}
