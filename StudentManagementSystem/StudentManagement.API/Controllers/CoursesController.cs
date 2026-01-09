using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Infrastructure.Data;

namespace StudentManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách tất cả môn học (để Agent chọn môn)
        [HttpGet]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _context.Courses
                .Select(c => new
                {
                    c.CourseId,
                    c.CourseName,
                    c.CourseCode,
                    c.Credits,
                    PrerequisiteId = c.PrerequisiteCourseId // Để Agent biết môn nào cần học trước
                })
                .ToListAsync();
            return Ok(courses);
        }

        // 2. Lấy chi tiết 1 môn (để check kỹ hơn)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();
            return Ok(course);
        }
    }
}
