using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using StudentManagement.Core.DTOs;
using StudentManagement.Core.Interfaces;

namespace StudentManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradesController : ControllerBase
    {
        private readonly IGradeService _gradeService;

        public GradesController(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateGrade([FromBody] UpdateGradeRequest request)
        {
            try
            {
                var result = await _gradeService.UpdateGradeAsync(request);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
