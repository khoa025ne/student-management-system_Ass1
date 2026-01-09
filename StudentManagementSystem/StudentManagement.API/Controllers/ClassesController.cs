using Microsoft.AspNetCore.Mvc;
using StudentManagement.Core.DTOs;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Interfaces;
using System.Threading.Tasks;

namespace StudentManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassesController : ControllerBase
    {
        private readonly IClassRepository _repo;

        public ClassesController(IClassRepository repo)
        {
            _repo = repo;
        }

        // GET: api/classes/semester/1
        [HttpGet("semester/{semesterId}")]
        public async Task<IActionResult> GetBySemester(int semesterId)
        {
            var classes = await _repo.GetBySemesterAsync(semesterId);
            return Ok(classes);
        }

        // POST: api/classes
        [HttpPost]
        public async Task<IActionResult> Create(CreateClassDto request)
        {
            // 1. Check trùng phòng
            bool isRoomAvailable = await _repo.CheckRoomAvailabilityAsync(request.Room, request.Schedule, request.SemesterId);
            if (!isRoomAvailable)
            {
                return BadRequest($"Phòng {request.Room} đã có lớp học vào ca {request.Schedule} trong kỳ này!");
            }

            var newClass = new Class
            {
                ClassName = request.ClassName,
                CourseId = request.CourseId,
                SemesterId = request.SemesterId,
                Room = request.Room,
                Schedule = request.Schedule,
                MaxCapacity = request.MaxCapacity,
                CurrentEnrollment = 0
            };

            await _repo.AddAsync(newClass);
            return Ok(new { Message = "Mở lớp thành công!", ClassId = newClass.ClassId });
        }
    }
}
