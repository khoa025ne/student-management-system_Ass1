using Microsoft.AspNetCore.Mvc;
using StudentManagement.Core.DTOs; // Nhớ tạo RegisterDto
using StudentManagement.Infrastructure.Services;
using System.Threading.Tasks;

namespace StudentManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly EnrollmentService _service;

        public EnrollmentsController(EnrollmentService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            var result = await _service.RegisterCourseAsync(request.StudentId, request.ClassId);

            if (result == "Success")
                return Ok(new { Message = "Đăng ký môn học thành công!" });

            return BadRequest(new { Message = result }); // Trả về lỗi chi tiết (Trùng lịch, đầy lớp...)
        }
    }

    
}
