using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Infrastructure.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; // Để dùng Any()

namespace StudentManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SeedController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================================================
        // 1. MASTER SEED: CHẠY TẤT CẢ (KỲ -> MÔN -> LỚP) CHỈ VỚI 1 NÚT BẤM
        // =========================================================================
        [HttpPost("seed-all")]
        public async Task<IActionResult> SeedAll([FromQuery] bool reset = false)
        {
            if (reset)
            {
                // Xóa theo thứ tự ngược: Lớp -> Môn -> Kỳ (để tránh lỗi Khóa ngoại)
                _context.Classes.RemoveRange(_context.Classes);
                _context.Courses.RemoveRange(_context.Courses);
                _context.Semesters.RemoveRange(_context.Semesters);
                await _context.SaveChangesAsync();
            }

            await SeedSemesters(); // 1. Tạo Kỳ
            await SeedCourses();   // 2. Tạo Môn
            await SeedClasses();   // 3. Tạo Lớp (phụ thuộc vào Kỳ & Môn)

            return Ok("Đã khởi tạo TOÀN BỘ dữ liệu mẫu (Kỳ, Môn, Lớp) thành công!");
        }


        // =========================================================================
        // 2. PRIVATE METHODS (LOGIC CHI TIẾT)
        // =========================================================================

        // --- SEED SEMESTERS ---
        private async Task SeedSemesters()
        {
            if (await _context.Semesters.AnyAsync()) return; // Đã có thì bỏ qua

            var semesters = new List<Semester>
            {
                // 2025
                new Semester { SemesterName = "Spring 2025", SemesterCode = "SPR25", StartDate = new DateTime(2025, 1, 6), EndDate = new DateTime(2025, 4, 30), IsActive = false },
                new Semester { SemesterName = "Summer 2025", SemesterCode = "SUM25", StartDate = new DateTime(2025, 5, 5), EndDate = new DateTime(2025, 8, 31), IsActive = false },
                new Semester { SemesterName = "Fall 2025",   SemesterCode = "FAL25", StartDate = new DateTime(2025, 9, 8), EndDate = new DateTime(2025, 12, 31), IsActive = false },
                // 2026
                new Semester { SemesterName = "Spring 2026", SemesterCode = "SPR26", StartDate = new DateTime(2026, 1, 5), EndDate = new DateTime(2026, 4, 30), IsActive = true }, // ACTIVE
                new Semester { SemesterName = "Summer 2026", SemesterCode = "SUM26", StartDate = new DateTime(2026, 5, 4), EndDate = new DateTime(2026, 8, 31), IsActive = false }
            };

            await _context.Semesters.AddRangeAsync(semesters);
            await _context.SaveChangesAsync();
        }

        // --- SEED COURSES ---
        private async Task SeedCourses()
        {
            if (await _context.Courses.AnyAsync()) return;

            // Môn cơ sở
            var prf192 = new Course { CourseCode = "PRF192", CourseName = "Programming Fundamentals with C", Credits = 3 };
            var mas291 = new Course { CourseCode = "MAS291", CourseName = "Statistics and Probability", Credits = 3 };
            await _context.Courses.AddRangeAsync(prf192, mas291);
            await _context.SaveChangesAsync(); // Lưu để lấy ID làm Prerequisite

            // Môn chuyên ngành
            var courses = new List<Course>
            {
                // SE
                new Course { CourseCode = "PRO192", CourseName = "Object-Oriented Programming (Java)", Credits = 3, PrerequisiteCourseId = prf192.CourseId },
                new Course { CourseCode = "PRN211", CourseName = "Basic Cross-Platform App Programming (.NET)", Credits = 3 },
                new Course { CourseCode = "SWT301", CourseName = "Software Testing", Credits = 3 },
                // AI
                new Course { CourseCode = "AIL302m", CourseName = "Artificial Intelligence Laboratory", Credits = 3, PrerequisiteCourseId = mas291.CourseId },
                new Course { CourseCode = "DPL302m", CourseName = "Deep Learning", Credits = 3 },
                // IA (SS)
                new Course { CourseCode = "ETH301",  CourseName = "Ethical Hacking", Credits = 3 }
            };

            await _context.Courses.AddRangeAsync(courses);
            await _context.SaveChangesAsync();
        }

        // --- SEED CLASSES (LỚP HỌC) ---
        private async Task SeedClasses()
        {
            if (await _context.Classes.AnyAsync()) return;

            // Tìm dữ liệu tham chiếu
            var spr26 = await _context.Semesters.FirstOrDefaultAsync(s => s.SemesterCode == "SPR26");
            var prf192 = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == "PRF192");
            var pro192 = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == "PRO192");
            var prn211 = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == "PRN211");

            if (spr26 == null || prf192 == null) return; // Chưa có data nền thì không tạo lớp

            var classes = new List<Class>
            {
                // Lớp C (PRF192)
                new Class { ClassName = "SE1801", CourseId = prf192.CourseId, SemesterId = spr26.SemesterId, Room = "BE-301", Schedule = "Mon-Wed-Fri (M1)", MaxCapacity = 30 },
                new Class { ClassName = "AI1802", CourseId = prf192.CourseId, SemesterId = spr26.SemesterId, Room = "DE-402", Schedule = "Tue-Thu-Sat (A1)", MaxCapacity = 35 },
                
                // Lớp Java (PRO192)
                new Class { ClassName = "SE1801", CourseId = pro192.CourseId, SemesterId = spr26.SemesterId, Room = "AL-205", Schedule = "Tue-Thu-Sat (M2)", MaxCapacity = 30 },
                
                // Lớp .NET (PRN211)
                new Class { ClassName = "SE1750", CourseId = prn211.CourseId, SemesterId = spr26.SemesterId, Room = "BE-501", Schedule = "Mon-Wed-Fri (E1)", MaxCapacity = 25 }
            };

            await _context.Classes.AddRangeAsync(classes);
            await _context.SaveChangesAsync();
        }
    }
}
