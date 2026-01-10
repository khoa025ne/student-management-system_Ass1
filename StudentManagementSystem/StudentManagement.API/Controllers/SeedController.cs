using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Enums;
using StudentManagement.Infrastructure.Data;
using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // =====================================================================
        // 1. MASTER SEED: Kỳ -> Môn -> Lớp -> User/Student -> Enrollment
        // =====================================================================
        [HttpPost("seed-all")]
        public async Task<IActionResult> SeedAll([FromQuery] bool reset = false)
        {
            if (reset)
            {
                _context.Enrollments.RemoveRange(_context.Enrollments);
                _context.Students.RemoveRange(_context.Students);
                _context.Users.RemoveRange(_context.Users);
                _context.Classes.RemoveRange(_context.Classes);
                _context.Courses.RemoveRange(_context.Courses);
                _context.Semesters.RemoveRange(_context.Semesters);
                await _context.SaveChangesAsync();
            }

            await SeedSemestersK18();
            await SeedCoursesIT_3Majors();  // SE, SS/IA, AI
            await SeedClassesWithSlots();   // Lớp có DayOfWeekPair + TimeSlot
            await SeedK18Students();        // Một vài SV đại diện SE/SS/AI
            await SeedEnrollmentsAndGrades();

            return Ok("Đã seed dữ liệu K18 (2022) cho 3 chuyên ngành SE/SS/AI.");
        }

        // =====================================================================
        // 2. SEMESTERS – lộ trình K18 từ 2022
        // =====================================================================
        private async Task SeedSemestersK18()
        {
            if (await _context.Semesters.AnyAsync()) return;

            var semesters = new List<Semester>
            {
                new Semester
                {
                    SemesterName = "Fall 2022",
                    SemesterCode = "FAL22",
                    StartDate = new DateTime(2022, 9, 5),
                    EndDate   = new DateTime(2022, 12, 31),
                    IsActive  = false
                },
                new Semester
                {
                    SemesterName = "Spring 2023",
                    SemesterCode = "SPR23",
                    StartDate = new DateTime(2023, 1, 3),
                    EndDate   = new DateTime(2023, 4, 30),
                    IsActive  = false
                },
                new Semester
                {
                    SemesterName = "Fall 2023",
                    SemesterCode = "FAL23",
                    StartDate = new DateTime(2023, 9, 4),
                    EndDate   = new DateTime(2023, 12, 31),
                    IsActive  = false
                },
                new Semester
                {
                    SemesterName = "Spring 2024",
                    SemesterCode = "SPR24",
                    StartDate = new DateTime(2024, 1, 2),
                    EndDate   = new DateTime(2024, 4, 30),
                    IsActive  = false
                },
                new Semester
                {
                    SemesterName = "Fall 2024",
                    SemesterCode = "FAL24",
                    StartDate = new DateTime(2024, 9, 2),
                    EndDate   = new DateTime(2024, 12, 31),
                    IsActive  = false
                },
                new Semester
                {
                    SemesterName = "Spring 2025",
                    SemesterCode = "SPR25",
                    StartDate = new DateTime(2025, 1, 6),
                    EndDate   = new DateTime(2025, 4, 30),
                    IsActive  = false
                },
                new Semester
                {
                    SemesterName = "Fall 2025",
                    SemesterCode = "FAL25",
                    StartDate = new DateTime(2025, 9, 8),
                    EndDate   = new DateTime(2025, 12, 31),
                    IsActive  = false
                },
                new Semester
                {
                    SemesterName = "Spring 2026",
                    SemesterCode = "SPR26",
                    StartDate = new DateTime(2026, 1, 5),
                    EndDate   = new DateTime(2026, 4, 30),
                    IsActive  = true        // kỳ hiện tại
                }
            };

            await _context.Semesters.AddRangeAsync(semesters);
            await _context.SaveChangesAsync();
        }

        // =====================================================================
        // 3. COURSES – core IT + 3 chuyên ngành SE/SS/AI (đÃ SỬA FK)
        // =====================================================================
        private async Task SeedCoursesIT_3Majors()
        {
            // Nếu đã có course thì bỏ qua, tránh seed trùng
            if (await _context.Courses.AnyAsync()) return;

            // ------------------------------
            // 1. Thêm các môn gốc (không có tiên quyết)
            // ------------------------------
            var prf192 = new Course
            {
                CourseCode = "PRF192",
                CourseName = "Programming Fundamentals",
                Credits = 3
            };

            var maa101 = new Course
            {
                CourseCode = "MAA101",
                CourseName = "Calculus 1",
                Credits = 3
            };

            var mas291 = new Course
            {
                CourseCode = "MAS291",
                CourseName = "Statistics and Probability",
                Credits = 3
            };

            await _context.Courses.AddRangeAsync(prf192, maa101, mas291);
            await _context.SaveChangesAsync();

            // Sau SaveChanges, chắc chắn đã có CourseId trong DB
            prf192 = await _context.Courses
                .FirstAsync(c => c.CourseCode == "PRF192");

            // ------------------------------
            // 2. Thêm các môn có tiên quyết trực tiếp là PRF192
            // ------------------------------
            var pro192 = new Course
            {
                CourseCode = "PRO192",
                CourseName = "Object-Oriented Programming (Java)",
                Credits = 3,
                PrerequisiteCourseId = prf192.CourseId
            };

            var iaa201 = new Course
            {
                CourseCode = "IAA201",
                CourseName = "Introduction to Information Assurance",
                Credits = 3,
                PrerequisiteCourseId = prf192.CourseId
            };

            var dsb201 = new Course
            {
                CourseCode = "DSB201",
                CourseName = "Introduction to Data Science",
                Credits = 3,
                PrerequisiteCourseId = prf192.CourseId
            };

            // SWD391 không set tiên quyết ở đây để tránh vòng FK phức tạp
            var swd391 = new Course
            {
                CourseCode = "SWD391",
                CourseName = "Software Architecture and Design",
                Credits = 3
            };

            await _context.Courses.AddRangeAsync(pro192, iaa201, dsb201, swd391);
            await _context.SaveChangesAsync();

            // Lấy lại từ DB để dùng CourseId chính xác
            iaa201 = await _context.Courses.FirstAsync(c => c.CourseCode == "IAA201");
            dsb201 = await _context.Courses.FirstAsync(c => c.CourseCode == "DSB201");

            // ------------------------------
            // 3. Thêm các môn nâng cao (AI / SS) dựa trên course đã tồn tại
            // ------------------------------
            var aic301 = new Course
            {
                CourseCode = "AIC301",
                CourseName = "Machine Learning Fundamentals",
                Credits = 3,
                PrerequisiteCourseId = dsb201.CourseId
            };

            var net212 = new Course
            {
                CourseCode = "NET212",
                CourseName = "Computer Networks and Security Basics",
                Credits = 3,
                PrerequisiteCourseId = iaa201.CourseId
            };

            await _context.Courses.AddRangeAsync(aic301, net212);
            await _context.SaveChangesAsync();
        }

        // =====================================================================
        // 4. CLASSES – tạo lớp có DayOfWeekPair + TimeSlot
        // =====================================================================
        private async Task SeedClassesWithSlots()
        {
            if (await _context.Classes.AnyAsync()) return;

            var fal22 = await _context.Semesters.FirstAsync(s => s.SemesterCode == "FAL22");
            var spr23 = await _context.Semesters.FirstAsync(s => s.SemesterCode == "SPR23");
            var spr26 = await _context.Semesters.FirstAsync(s => s.SemesterCode == "SPR26");

            var prf192 = await _context.Courses.FirstAsync(c => c.CourseCode == "PRF192");
            var pro192 = await _context.Courses.FirstAsync(c => c.CourseCode == "PRO192");
            var swd391 = await _context.Courses.FirstAsync(c => c.CourseCode == "SWD391");

            var iaa201 = await _context.Courses.FirstAsync(c => c.CourseCode == "IAA201");
            var net212 = await _context.Courses.FirstAsync(c => c.CourseCode == "NET212");

            var dsb201 = await _context.Courses.FirstAsync(c => c.CourseCode == "DSB201");
            var aic301 = await _context.Courses.FirstAsync(c => c.CourseCode == "AIC301");

            var classes = new List<Class>
            {
                // FAL22 – core PRF192 cho cả 3 chuyên ngành
                new Class
                {
                    ClassCode = "SE_K18_PRF192_1",
                    ClassName = "PRF192 SE K18.1",
                    CourseId  = prf192.CourseId,
                    SemesterId = fal22.SemesterId,
                    Room = "BE-201",
                    Schedule = "Mon-Wed-Fri (M1)",
                    DayOfWeekPair = DayOfWeekPair.MonThu,
                    TimeSlot      = TimeSlot.Slot1,
                    MaxCapacity   = 35
                },

                // SPR23 – Java cho SE
                new Class
                {
                    ClassCode = "SE_K18_PRO192_1",
                    ClassName = "PRO192 SE K18.1",
                    CourseId  = pro192.CourseId,
                    SemesterId = spr23.SemesterId,
                    Room = "AL-205",
                    Schedule = "Tue-Thu-Sat (M2)",
                    DayOfWeekPair = DayOfWeekPair.TueFri,
                    TimeSlot      = TimeSlot.Slot2,
                    MaxCapacity   = 35
                },

                // SPR23 – IA intro cho SS
                new Class
                {
                    ClassCode = "SS_K18_IAA201_1",
                    ClassName = "IAA201 SS K18.1",
                    CourseId  = iaa201.CourseId,
                    SemesterId = spr23.SemesterId,
                    Room = "BE-305",
                    Schedule = "Mon-Wed-Fri (M3)",
                    DayOfWeekPair = DayOfWeekPair.MonThu,
                    TimeSlot      = TimeSlot.Slot3,
                    MaxCapacity   = 35
                },

                // SPR23 – DS intro cho AI
                new Class
                {
                    ClassCode = "AI_K18_DSB201_1",
                    ClassName = "DSB201 AI K18.1",
                    CourseId  = dsb201.CourseId,
                    SemesterId = spr23.SemesterId,
                    Room = "BE-402",
                    Schedule = "Tue-Thu-Sat (M3)",
                    DayOfWeekPair = DayOfWeekPair.TueFri,
                    TimeSlot      = TimeSlot.Slot3,
                    MaxCapacity   = 35
                },

                // SPR26 – lớp hiện tại (advanced SE/SS/AI) để test đăng ký
                new Class
                {
                    ClassCode = "SE_K18_SWD391_1",
                    ClassName = "SWD391 SE K18.1",
                    CourseId  = swd391.CourseId,
                    SemesterId = spr26.SemesterId,
                    Room = "BE-501",
                    Schedule = "Mon-Wed-Fri (E1)",
                    DayOfWeekPair = DayOfWeekPair.MonThu,
                    TimeSlot      = TimeSlot.Slot4,
                    MaxCapacity   = 30
                },
                new Class
                {
                    ClassCode = "SS_K18_NET212_1",
                    ClassName = "NET212 SS K18.1",
                    CourseId  = net212.CourseId,
                    SemesterId = spr26.SemesterId,
                    Room = "BE-502",
                    Schedule = "Tue-Thu-Sat (E1)",
                    DayOfWeekPair = DayOfWeekPair.TueFri,
                    TimeSlot      = TimeSlot.Slot4,
                    MaxCapacity   = 30
                },
                new Class
                {
                    ClassCode = "AI_K18_AIC301_1",
                    ClassName = "AIC301 AI K18.1",
                    CourseId  = aic301.CourseId,
                    SemesterId = spr26.SemesterId,
                    Room = "BE-503",
                    Schedule = "Wed-Sat (E1)",
                    DayOfWeekPair = DayOfWeekPair.WedSat,
                    TimeSlot      = TimeSlot.Slot4,
                    MaxCapacity   = 30
                }
            };

            await _context.Classes.AddRangeAsync(classes);
            await _context.SaveChangesAsync();
        }

        // =====================================================================
        // 5. STUDENTS – 3 sinh viên đại diện SE / SS / AI (K18, nhập 2022)
        // =====================================================================
        private async Task SeedK18Students()
        {
            if (await _context.Students.AnyAsync()) return;

            var passwordHash = BCrypt.Net.BCrypt.HashPassword("01012004"); // ví dụ mật khẩu default

            // TẠO USER: BẮT BUỘC GÁN PhoneNumber (không được null)
            var users = new List<User>
    {
        new User
        {
            Email        = "se_k18_001@fpt.edu.vn",
            FullName     = "Nguyen Van SE",
            PasswordHash = passwordHash,
            RoleId       = 4,
            IsActive     = true,
            PhoneNumber  = "0901000001"
        },
        new User
        {
            Email        = "ss_k18_001@fpt.edu.vn",
            FullName     = "Tran Thi SS",
            PasswordHash = passwordHash,
            RoleId       = 4,
            IsActive     = true,
            PhoneNumber  = "0901000002"
        },
        new User
        {
            Email        = "ai_k18_001@fpt.edu.vn",
            FullName     = "Le Van AI",
            PasswordHash = passwordHash,
            RoleId       = 4,
            IsActive     = true,
            PhoneNumber  = "0901000003"
        }
    };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // TẠO STUDENT liên kết với User
            var students = new List<Student>
    {
        new Student
        {
            StudentCode   = "SE180001",
            FullName      = "Nguyen Van SE",
            Email         = "se_k18_001@fpt.edu.vn",
            PhoneNumber   = "0901000001",
            DateOfBirth   = new DateTime(2004, 1, 1),
            Major         = "SE",
            ClassCode     = "SE1801",
            OverallGPA    = 0,
            UserId        = users[0].UserId,
            CurrentTermNo = 7,
            IsFirstLogin  = true
        },
        new Student
        {
            StudentCode   = "SS180001",
            FullName      = "Tran Thi SS",
            Email         = "ss_k18_001@fpt.edu.vn",
            PhoneNumber   = "0901000002",
            DateOfBirth   = new DateTime(2004, 2, 2),
            Major         = "SS",
            ClassCode     = "SS1801",
            OverallGPA    = 0,
            UserId        = users[1].UserId,
            CurrentTermNo = 7,
            IsFirstLogin  = true
        },
        new Student
        {
            StudentCode   = "AI180001",
            FullName      = "Le Van AI",
            Email         = "ai_k18_001@fpt.edu.vn",
            PhoneNumber   = "0901000003",
            DateOfBirth   = new DateTime(2004, 3, 3),
            Major         = "AI",
            ClassCode     = "AI1801",
            OverallGPA    = 0,
            UserId        = users[2].UserId,
            CurrentTermNo = 7,
            IsFirstLogin  = true
        }
    };

            await _context.Students.AddRangeAsync(students);
            await _context.SaveChangesAsync();
        }


        // =====================================================================
        // 6. ENROLLMENTS – lịch sử + hiện tại cho 3 SV
        // =====================================================================
        private async Task SeedEnrollmentsAndGrades()
        {
            if (await _context.Enrollments.AnyAsync()) return;

            var se = await _context.Students.FirstAsync(s => s.StudentCode == "SE180001");
            var ss = await _context.Students.FirstAsync(s => s.StudentCode == "SS180001");
            var ai = await _context.Students.FirstAsync(s => s.StudentCode == "AI180001");

            var classPRF = await _context.Classes.FirstAsync(c => c.ClassCode == "SE_K18_PRF192_1");
            var classPRO = await _context.Classes.FirstAsync(c => c.ClassCode == "SE_K18_PRO192_1");
            var classSWD = await _context.Classes.FirstAsync(c => c.ClassCode == "SE_K18_SWD391_1");

            var classIAA = await _context.Classes.FirstAsync(c => c.ClassCode == "SS_K18_IAA201_1");
            var classNET = await _context.Classes.FirstAsync(c => c.ClassCode == "SS_K18_NET212_1");

            var classDSB = await _context.Classes.FirstAsync(c => c.ClassCode == "AI_K18_DSB201_1");
            var classAIC = await _context.Classes.FirstAsync(c => c.ClassCode == "AI_K18_AIC301_1");

            var enrollments = new List<Enrollment>
            {
                // SE: đã học PRF192 + PRO192, đang học SWD391
                new Enrollment { StudentId = se.StudentId, ClassId = classPRF.ClassId, Status = "Completed", AttemptNumber = 1, EnrollmentDate = classPRF.Semester.StartDate.AddDays(-7), MidtermScore = 8, FinalScore = 9, TotalScore = 8.6, Grade = "A", IsPassed = true },
                new Enrollment { StudentId = se.StudentId, ClassId = classPRO.ClassId, Status = "Completed", AttemptNumber = 1, EnrollmentDate = classPRO.Semester.StartDate.AddDays(-7), MidtermScore = 7.5, FinalScore = 8, TotalScore = 7.8, Grade = "B+", IsPassed = true },
                new Enrollment { StudentId = se.StudentId, ClassId = classSWD.ClassId, Status = "Active", AttemptNumber = 1, EnrollmentDate = DateTime.Now, IsPassed = false },

                // SS: đã học PRF192 + IAA201, đang học NET212
                new Enrollment { StudentId = ss.StudentId, ClassId = classPRF.ClassId, Status = "Completed", AttemptNumber = 1, EnrollmentDate = classPRF.Semester.StartDate.AddDays(-7), MidtermScore = 7, FinalScore = 8, TotalScore = 7.5, Grade = "B", IsPassed = true },
                new Enrollment { StudentId = ss.StudentId, ClassId = classIAA.ClassId, Status = "Completed", AttemptNumber = 1, EnrollmentDate = classIAA.Semester.StartDate.AddDays(-7), MidtermScore = 8, FinalScore = 8.5, TotalScore = 8.3, Grade = "A", IsPassed = true },
                new Enrollment { StudentId = ss.StudentId, ClassId = classNET.ClassId, Status = "Active", AttemptNumber = 1, EnrollmentDate = DateTime.Now, IsPassed = false },

                // AI: đã học PRF192 + DSB201, đang học AIC301
                new Enrollment { StudentId = ai.StudentId, ClassId = classPRF.ClassId, Status = "Completed", AttemptNumber = 1, EnrollmentDate = classPRF.Semester.StartDate.AddDays(-7), MidtermScore = 9, FinalScore = 9, TotalScore = 9, Grade = "A+", IsPassed = true },
                new Enrollment { StudentId = ai.StudentId, ClassId = classDSB.ClassId, Status = "Completed", AttemptNumber = 1, EnrollmentDate = classDSB.Semester.StartDate.AddDays(-7), MidtermScore = 8.5, FinalScore = 8.5, TotalScore = 8.5, Grade = "A", IsPassed = true },
                new Enrollment { StudentId = ai.StudentId, ClassId = classAIC.ClassId, Status = "Active", AttemptNumber = 1, EnrollmentDate = DateTime.Now, IsPassed = false }
            };

            await _context.Enrollments.AddRangeAsync(enrollments);

            // cập nhật sĩ số lớp
            foreach (var g in enrollments.GroupBy(e => e.ClassId))
            {
                var cls = await _context.Classes.FindAsync(g.Key);
                cls.CurrentEnrollment += g.Count();
            }

            await _context.SaveChangesAsync();
        }
    }
}
