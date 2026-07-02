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

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTimetable(int id, [FromBody] UpdateTimetableDTO dto, CancellationToken cancellationToken)
        {
            var result = await _timetableService.UpdateTimetableAsync(id, dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet("teacher/{teacherId}")]
        public async Task<IActionResult> GetTeacherTimetable(int teacherId, CancellationToken cancellationToken)
        {
            var result = await _timetableService.GetTeacherTimetableAsync(teacherId, cancellationToken);
            return Ok(result);
        }
        [HttpGet("teacher-requirements")]
        public async Task<ActionResult<IReadOnlyList<TeacherRequirementDTO>>> GetTeacherRequirements([FromQuery] int periodsPerDay, [FromQuery] int freePeriodsPerStaff, CancellationToken cancellationToken)
        {
            var result = await _timetableService.GetTeacherRequirementsAsync(periodsPerDay, freePeriodsPerStaff, cancellationToken);
            return Ok(result);
        }

        [HttpPost("generate")]
        public async Task<ActionResult<IReadOnlyList<TimetableResponseDTO>>> GenerateTimetable([FromBody] GenerateTimetableRequestDTO request, CancellationToken cancellationToken)
        {
            var result = await _timetableService.GenerateTimetableAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("save-generated")]
        public async Task<IActionResult> SaveGeneratedTimetable([FromBody] IReadOnlyList<TimetableResponseDTO> generatedTimetable, CancellationToken cancellationToken)
        {
            await _timetableService.SaveGeneratedTimetableAsync(generatedTimetable, cancellationToken);
            return Ok();
        }
    }
}
