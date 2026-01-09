using Microsoft.AspNetCore.Mvc;
using StudentManagement.Core.DTOs;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Interfaces;
using System.Threading.Tasks;

namespace StudentManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SemestersController : ControllerBase
    {
        private readonly ISemesterRepository _repo;

        public SemestersController(ISemesterRepository repo)
        {
            _repo = repo;
        }

        // GET: api/semesters
        // Lấy danh sách tất cả học kỳ
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var semesters = await _repo.GetAllAsync();
            return Ok(semesters);
        }

        // POST: api/semesters
        // Tạo học kỳ mới
        [HttpPost]
        public async Task<IActionResult> Create(CreateSemesterDto request)
        {
            // Validate ngày tháng
            if (request.StartDate >= request.EndDate)
            {
                return BadRequest("Ngày bắt đầu phải trước ngày kết thúc.");
            }

            var newSemester = new Semester
            {
                SemesterName = request.SemesterName,
                SemesterCode = request.SemesterCode,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = false // Mặc định chưa kích hoạt
            };

            await _repo.AddAsync(newSemester);

            return Ok(new
            {
                Message = "Tạo học kỳ thành công!",
                SemesterId = newSemester.SemesterId
            });
        }

        // PUT: api/semesters/5/set-active
        // Kích hoạt học kỳ hiện tại
        [HttpPut("{id}/set-active")]
        public async Task<IActionResult> SetActive(int id)
        {
            await _repo.SetActiveSemesterAsync(id);
            return Ok(new { Message = $"Đã kích hoạt học kỳ ID {id} là kỳ hiện tại." });
        }
    }
}
