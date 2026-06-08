using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.Timetable;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TimetableController : ControllerBase
    {
        private readonly ITimetableService _timetableService;

        public TimetableController(ITimetableService timetableService)
        {
            _timetableService = timetableService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTimetable([FromBody] CreateTimetableDTO dto, CancellationToken cancellationToken)
        {
            var result = await _timetableService.CreateTimetableAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetClassTimetable(int classId, CancellationToken cancellationToken)
        {
            var result = await _timetableService.GetClassTimetableAsync(classId, cancellationToken);
            return Ok(result);
        }
    }
}
