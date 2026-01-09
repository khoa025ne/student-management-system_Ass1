using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Cần thiết để check trùng
using StudentManagement.Core.DTOs;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Interfaces;
using StudentManagement.Infrastructure.Data; // Cần truy cập DbContext để check trùng
using BCrypt.Net; // Thư viện Hash

namespace StudentManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentRepository _repo;
        private readonly AppDbContext _context; // Cần context để check trùng mã SV

        public StudentsController(IStudentRepository repo, AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }




        [HttpPost("upload-avatar/{studentId}")]
        public async Task<IActionResult> UploadAvatar(int studentId, IFormFile file)
        {
            // 1. Validate File có tồn tại không
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file ảnh.");

            // 2. Validate Dung lượng (< 1MB)
            // 1 MB = 1024 * 1024 bytes
            if (file.Length > 1024 * 1024)
                return BadRequest("Dung lượng ảnh quá lớn. Vui lòng chọn ảnh dưới 1MB.");

            // 3. Validate đuôi file (Chỉ cho phép .jpg, .png)
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".jpg" && ext != ".png" && ext != ".jpeg")
                return BadRequest("Chỉ chấp nhận file ảnh (.jpg, .png)");

            // 4. Lưu file vào server
            var fileName = $"avatar_{studentId}_{DateTime.Now.Ticks}{ext}";
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 5. Cập nhật URL vào Database
            var student = await _context.Students.FindAsync(studentId);
            if (student != null)
            {
                student.AvatarUrl = $"/uploads/{fileName}";
                await _context.SaveChangesAsync();
            }

            return Ok(new { Url = student.AvatarUrl });
        }


        // ... (Các code cũ: constructor, create student, upload avatar giữ nguyên)

        // API 1: Lấy danh sách sinh viên (Để Agent biết ID nào mà test)
        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _context.Students
                .Select(s => new { s.StudentId, s.FullName, s.StudentCode, s.AvatarUrl })
                .ToListAsync();
            return Ok(students);
        }

        // API 2: Xem chi tiết sinh viên (Để check Avatar đã lên chưa)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound("Sinh viên không tồn tại");
            return Ok(student);
        }

        // API 3: Xem lịch sử đăng ký (Để check Attempt Number & Pass/Fail)
        [HttpGet("{id}/enrollments")]
        public async Task<IActionResult> GetStudentEnrollments(int id)
        {
            var history = await _context.Enrollments
                .Include(e => e.Class).ThenInclude(c => c.Course)
                .Where(e => e.StudentId == id)
                .OrderByDescending(e => e.EnrollmentDate)
                .Select(e => new
                {
                    e.EnrollmentId,
                    CourseName = e.Class.Course.CourseName,
                    CourseCode = e.Class.Course.CourseCode,
                    e.AttemptNumber, // Quan trọng để test Case 2
                    e.TotalScore,
                    e.IsPassed,      // Quan trọng để test Case 1
                    e.EnrollmentDate
                })
                .ToListAsync();

            return Ok(history);
        }

        // API 4: Bảng điểm & GPA (Giải quyết vấn đề GPA & Lần học cuối)
        [HttpGet("{id}/transcript")]
        public async Task<IActionResult> GetTranscript(int id)
        {
            // 1. Lấy tất cả các môn đã có điểm
            var allEnrollments = await _context.Enrollments
                .Include(e => e.Class).ThenInclude(c => c.Course)
                .Include(e => e.Class).ThenInclude(c => c.Semester)
                .Where(e => e.StudentId == id && e.TotalScore != null) // Chỉ lấy môn đã có điểm
                .ToListAsync();

            if (!allEnrollments.Any()) return Ok("Sinh viên chưa có điểm môn nào.");

            // 2. Logic tính GPA Tổng: Lấy điểm của lần học cuối cùng cho mỗi môn
            var latestAttempts = allEnrollments
                .GroupBy(e => e.Class.CourseId)
                .Select(g => g.OrderByDescending(e => e.EnrollmentDate).First())
                .ToList();

            double totalPoints = latestAttempts.Sum(e => (e.TotalScore ?? 0) * e.Class.Course.Credits);
            int totalCredits = latestAttempts.Sum(e => e.Class.Course.Credits);
            double gpa = totalCredits > 0 ? Math.Round(totalPoints / totalCredits, 2) : 0;

            // 3. Logic hiển thị theo từng kỳ
            var transcriptBySemester = allEnrollments
                .GroupBy(e => e.Class.Semester.SemesterName)
                .Select(g => new
                {
                    Semester = g.Key,
                    Courses = g.Select(e => new
                    {
                        Name = e.Class.Course.CourseName,
                        Score = e.TotalScore,
                        Pass = e.IsPassed
                    })
                });

            return Ok(new
            {
                StudentId = id,
                CumulativeGPA = gpa, // GPA Tích lũy
                Detail = transcriptBySemester
            });
        }



        // POST: api/students
        [HttpPost]
        public async Task<IActionResult> Create(CreateStudentDto request)
        {
            // 1. Tính toán Khóa học (Cohort) dựa trên năm nhập học
            // Giả sử mốc bắt đầu của trường là 2006 (Khóa 1).
            // Năm 2024 -> Khóa 18 (2024 - 2006 = 18). Bạn có thể chỉnh mốc này.
            int startYear = 2006;
            int currentYear = DateTime.Now.Year;
            int cohort = currentYear - startYear; // Ví dụ: 18

            // 2. Sinh mã sinh viên (Format: SE + 18 + Random 4 số)
            string majorCode = request.Major.ToUpper(); // SE
            string studentCode = await GenerateUniqueStudentCodeAsync(majorCode, cohort);

            // 3. Hash Password (Mặc định lấy ngày sinh ddmmyyyy làm pass)
            string defaultPassword = request.DateOfBirth.ToString("ddMMyyyy");
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);

            // 4. Tạo User
            var newUser = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = passwordHash, // Lưu pass đã hash
                RoleId = 4, // Role Student
                CreatedAt = DateTime.Now,
                IsActive = true,
                PhoneNumber = request.PhoneNumber
            };

            // 5. Tạo Student
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
            };

            await _repo.AddAsync(newStudent);
            return Ok(new
            {
                Message = "Tạo sinh viên thành công!",
                StudentCode = newStudent.StudentCode,
                DefaultPassword = defaultPassword // Trả về để test, sau này xóa đi
            });
        }

        // Hàm sinh mã và check trùng (Loop cho đến khi tìm được số chưa trùng)
        private async Task<string> GenerateUniqueStudentCodeAsync(string major, int cohort)
        {
            Random random = new Random();
            string code;
            bool isExist;

            do
            {
                // Random 4 số từ 0000 đến 9999
                int randomNum = random.Next(0, 9999);
                string randomStr = randomNum.ToString("D4"); // Đảm bảo đủ 4 chữ số (0012)

                // Ghép chuỗi: SE + 18 + 0012 => SE180012
                code = $"{major}{cohort}{randomStr}";

                // Kiểm tra trong DB xem có ai dùng mã này chưa
                isExist = await _context.Students.AnyAsync(s => s.StudentCode == code);

            } while (isExist); // Nếu trùng thì lặp lại random số khác

            return code;
        }
    }
}
