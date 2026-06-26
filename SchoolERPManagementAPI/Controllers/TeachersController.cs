using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.Teacher;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly IPdfReportService _pdfReportService;

        public TeachersController(ITeacherService teacherService, IPdfReportService pdfReportService)
        {
            _teacherService = teacherService;
            _pdfReportService = pdfReportService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTeachers([FromQuery] TeacherQueryRequest request, CancellationToken cancellationToken)
        {
            var result = await _teacherService.GetAllTeachersAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("export/pdf")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> ExportTeachersPdf([FromQuery] TeacherQueryRequest request, CancellationToken cancellationToken)
        {
            request.PageNumber = 1;
            request.PageSize = int.MaxValue;
            var result = await _teacherService.GetAllTeachersAsync(request, cancellationToken);
            var pdfBytes = _pdfReportService.GenerateTeachersPdf(result.Items.ToList());
            return File(pdfBytes, "application/pdf", "teachers-directory.pdf");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacherById(int id, CancellationToken cancellationToken)
        {
            var result = await _teacherService.GetTeacherByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetTeacherByUsername(string username, CancellationToken cancellationToken)
        {
            var result = await _teacherService.GetTeacherByUsernameAsync(username, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddTeacher([FromBody] CreateTeacherDTO dto, CancellationToken cancellationToken)
        {
            var result = await _teacherService.AddTeacherAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpPost("assign-subject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignSubject([FromBody] AssignTeacherSubjectDTO dto, CancellationToken cancellationToken)
        {
            var result = await _teacherService.AssignSubjectAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}/assignments")]
        public async Task<IActionResult> GetTeacherAssignments(int id, CancellationToken cancellationToken)
        {
            var result = await _teacherService.GetTeacherAssignmentsAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id}/assignments/{classId}/{subjectId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTeacherAssignment(int id, int classId, int subjectId, CancellationToken cancellationToken)
        {
            await _teacherService.DeleteTeacherAssignmentAsync(id, classId, subjectId, cancellationToken);
            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTeacher(int id, [FromBody] UpdateTeacherDTO dto, CancellationToken cancellationToken)
        {
            var result = await _teacherService.UpdateTeacherAsync(id, dto, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTeacher(int id, CancellationToken cancellationToken)
        {
            await _teacherService.DeleteTeacherAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
