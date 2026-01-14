using Microsoft.AspNetCore.Mvc;
using StudentManagement.Core.DTOs;
using StudentManagement.Infrastructure.Services;
using System.Threading.Tasks;

namespace StudentManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly EnrollmentService _service;

        // ĐÂY: định nghĩa DTO cho đổi lớp
        public class ChangeClassRequest
        {
            public int StudentId { get; set; }
            public int OldClassId { get; set; }
            public int NewClassId { get; set; }
        }

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

            return BadRequest(new { Message = result });
        }


        [HttpPost("change-class")]
        public async Task<IActionResult> ChangeClass([FromBody] ChangeClassRequest request)
        {
            var result = await _service.ChangeClassAsync(
                request.StudentId,
                request.OldClassId,
                request.NewClassId
            );

            if (result == "Success")
                return Ok(new { Message = "Đổi lớp thành công!" });

            return BadRequest(new { Message = result });
        }
    }
}
