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
        [HttpGet("semester/{semesterId:int}")]
        public async Task<IActionResult> GetBySemester(int semesterId)
        {
            var classes = await _repo.GetBySemesterAsync(semesterId);
            return Ok(classes);
        }

        // GET: api/classes/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cls = await _repo.GetByIdAsync(id);
            if (cls == null) return NotFound();
            return Ok(cls);
        }

        // POST: api/classes
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClassDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Nếu bạn còn muốn check trùng phòng theo slot + cặp ngày,
            // hãy sửa lại IClassRepository.CheckRoomAvailabilityAsync
            // để dùng DayOfWeekPair + TimeSlot thay vì Schedule.
            var isRoomAvailable = await _repo.CheckRoomAvailabilityAsync(
                request.Room,
                request.SemesterId,
                request.DayOfWeekPair,
                request.TimeSlot
            );

            if (!isRoomAvailable)
            {
                return BadRequest(
                    $"Phòng {request.Room} đã có lớp trong Slot {request.TimeSlot} - {request.DayOfWeekPair} của kỳ này!"
                );
            }

            var newClass = new Class
            {
                ClassName = request.ClassName,
                CourseId = request.CourseId,
                SemesterId = request.SemesterId,
                Room = request.Room,
                Schedule = request.Schedule,   // legacy, có thể để null
                MaxCapacity = request.MaxCapacity,
                CurrentEnrollment = 0,
                DayOfWeekPair = request.DayOfWeekPair,
                TimeSlot = request.TimeSlot
            };

            await _repo.AddAsync(newClass);

            return Ok(new
            {
                Message = "Mở lớp thành công!",
                ClassId = newClass.ClassId
            });
        }
    }
}
