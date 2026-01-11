using Microsoft.AspNetCore.Mvc;
using StudentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace StudentManagement.API.Controllers
{
    public class NotificationController
    {
        [Route("api/[controller]")]
        [ApiController]
        public class NotificationsController : ControllerBase
        {
            private readonly AppDbContext _context;

            public NotificationsController(AppDbContext context)
            {
                _context = context;
            }

            [HttpGet("student/{studentId:int}")]
            public async Task<IActionResult> GetForStudent(int studentId)
            {
                var list = await _context.Notifications
                    .Where(n => n.StudentId == studentId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                return Ok(list);
            }

            [HttpPost("{id:int}/read")]
            public async Task<IActionResult> MarkRead(int id)
            {
                var noti = await _context.Notifications.FindAsync(id);
                if (noti == null) return NotFound();
                noti.IsRead = true;
                await _context.SaveChangesAsync();
                return Ok();
            }
        }

    }
}
