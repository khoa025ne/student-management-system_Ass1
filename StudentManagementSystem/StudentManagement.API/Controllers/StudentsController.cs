using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.DTOs;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Interfaces;
using StudentManagement.Infrastructure.Data;
using BCrypt.Net;
using System.IO;

namespace StudentManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentRepository _repo;
        private readonly AppDbContext _context;

        public StudentsController(IStudentRepository repo, AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        // 1. Lấy danh sách sinh viên
        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _context.Students
                .Select(s => new
                {
                    s.StudentId,
                    s.FullName,
                    s.StudentCode,
                    s.AvatarUrl,
                    s.OverallGPA
                })
                .ToListAsync();

            return Ok(students);
        }

        // 2. Lấy chi tiết sinh viên
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound("Sinh viên không tồn tại");
            return Ok(student);
        }

        // 3. Tạo Sinh viên mới
        [HttpPost]
        public async Task<IActionResult> Create(CreateStudentDto request)
        {
            int startYear = 2006;
            int currentYear = DateTime.Now.Year;
            int cohort = currentYear - startYear;

            string majorCode = request.Major.ToUpper();
            string studentCode = await GenerateUniqueStudentCodeAsync(majorCode, cohort);

            string defaultPassword = request.DateOfBirth.ToString("ddMMyyyy");
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);

            var newUser = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = passwordHash,
                RoleId = 4,
                CreatedAt = DateTime.Now,
                IsActive = true,
                PhoneNumber = request.PhoneNumber
            };

            var newStudent = new Student
            {
                StudentCode = studentCode,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                Major = majorCode,
                ClassCode = request.ClassCode,
                OverallGPA = 0,
                User = newUser
                // CurrentTermNo, IsFirstLogin dùng mặc định từ entity
            };

            await _repo.AddAsync(newStudent);

            return Ok(new
            {
                Message = "Tạo sinh viên thành công!",
                StudentCode = newStudent.StudentCode,
                DefaultPassword = defaultPassword
            });
        }

        // 4. Upload Avatar
        [HttpPost("upload-avatar/{studentId:int}")]
        public async Task<IActionResult> UploadAvatar(int studentId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file ảnh.");

            if (file.Length > 2 * 1024 * 1024)
                return BadRequest("Dung lượng ảnh quá lớn (<2MB).");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".jpg" && ext != ".png" && ext != ".jpeg")
                return BadRequest("Chỉ chấp nhận file ảnh (.jpg, .png)");

            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return NotFound("Sinh viên không tồn tại");

            var uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadFolderPath)) Directory.CreateDirectory(uploadFolderPath);

            if (!string.IsNullOrEmpty(student.AvatarUrl))
            {
                var oldFileName = Path.GetFileName(student.AvatarUrl);
                var oldFilePath = Path.Combine(uploadFolderPath, oldFileName);
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);
            }

            var newFileName = $"avatar_{studentId}_{DateTime.Now.Ticks}{ext}";
            var newFilePath = Path.Combine(uploadFolderPath, newFileName);

            using (var stream = new FileStream(newFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            student.AvatarUrl = $"/uploads/{newFileName}";
            await _context.SaveChangesAsync();

            return Ok(new { Url = student.AvatarUrl, Message = "Upload và cập nhật ảnh thành công!" });
        }

        // 5. Xem Lịch học (Schedule) – ĐÃ THÊM DayOfWeekPair + TimeSlot
        [HttpGet("{id:int}/schedule")]
        public async Task<IActionResult> GetStudentSchedule(int id, [FromQuery] int? semesterId)
        {
            var query = _context.Enrollments
                .Include(e => e.Class).ThenInclude(c => c.Course)
                .Include(e => e.Class).ThenInclude(c => c.Semester)
                .Where(e => e.StudentId == id && e.Status == "Active");

            if (semesterId.HasValue)
            {
                query = query.Where(e => e.Class.SemesterId == semesterId.Value);
            }

            var schedule = await query
                .Select(e => new
                {
                    e.EnrollmentId,
                    e.Class.ClassId,
                    e.Class.ClassName,
                    e.Class.Room,
                    e.Class.Schedule,          // legacy text nếu bạn muốn hiển thị
                    e.Class.DayOfWeekPair,     // cặp ngày 2-5/3-6/4-7
                    e.Class.TimeSlot,          // Slot1–4
                    CourseCode = e.Class.Course.CourseCode,
                    CourseName = e.Class.Course.CourseName,
                    Semester = e.Class.Semester.SemesterName
                })
                .ToListAsync();

            return Ok(schedule);
        }

        // 6. Xem Bảng điểm chi tiết (Transcript) – giữ nguyên như file cũ
        [HttpGet("{id:int}/transcript")]
        public async Task<IActionResult> GetTranscript(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound("Sinh viên không tồn tại");

            var allEnrollments = await _context.Enrollments
                .Include(e => e.Class).ThenInclude(c => c.Course)
                .Include(e => e.Class).ThenInclude(c => c.Semester)
                .Where(e => e.StudentId == id && e.TotalScore != null)
                .OrderBy(e => e.Class.Semester.StartDate)
                .ToListAsync();

            var transcriptData = allEnrollments
                .GroupBy(e => e.Class.Semester.SemesterName)
                .Select(g => new
                {
                    Semester = g.Key,
                    Courses = g.Select(e => new
                    {
                        CourseCode = e.Class.Course.CourseCode,
                        CourseName = e.Class.Course.CourseName,
                        Credits = e.Class.Course.Credits,
                        Score = e.TotalScore,
                        Grade = e.Grade,
                        Status = e.IsPassed ? "Passed" : "Failed"
                    }),
                    SemesterGPA = CalculateSemesterGpa(g.ToList())
                });

            return Ok(new
            {
                StudentCode = student.StudentCode,
                FullName = student.FullName,
                OverallGPA = student.OverallGPA,
                Details = transcriptData
            });
        }

        // --- HELPER FUNCTIONS ---

        private double CalculateSemesterGpa(List<Enrollment> enrollments)
        {
            double totalPoints = enrollments.Sum(e => (e.TotalScore ?? 0) * e.Class.Course.Credits);
            int totalCredits = enrollments.Sum(e => e.Class.Course.Credits);
            return totalCredits > 0 ? Math.Round(totalPoints / totalCredits, 3) : 0;
        }

        private async Task<string> GenerateUniqueStudentCodeAsync(string major, int cohort)
        {
            Random random = new Random();
            string code;
            bool isExist;

            do
            {
                int randomNum = random.Next(0, 9999);
                code = $"{major}{cohort}{randomNum:D4}";
                isExist = await _context.Students.AnyAsync(s => s.StudentCode == code);
            } while (isExist);

            return code;
        }
    }
}
