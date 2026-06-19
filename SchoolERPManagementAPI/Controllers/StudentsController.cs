using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.Student;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IParentService _parentService;
        private readonly IPdfReportService _pdfReportService;

        public StudentsController(IStudentService studentService, IParentService parentService, IPdfReportService pdfReportService)
        {
            _studentService = studentService;
            _parentService = parentService;
            _pdfReportService = pdfReportService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAllStudents([FromQuery] StudentQueryRequest request, CancellationToken cancellationToken)
        {
            var result = await _studentService.GetAllStudentsAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("export/pdf")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> ExportStudentsPdf([FromQuery] StudentQueryRequest request, CancellationToken cancellationToken)
        {
            request.PageNumber = 1;
            request.PageSize = int.MaxValue;
            var result = await _studentService.GetAllStudentsAsync(request, cancellationToken);
            var pdfBytes = _pdfReportService.GenerateStudentsPdf(result.Items.ToList());
            return File(pdfBytes, "application/pdf", "students-directory.pdf");
        }

        [HttpGet("class/{classId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetStudentsByClassId(int classId, CancellationToken cancellationToken)
        {
            var result = await _studentService.GetStudentsByClassIdAsync(classId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudentById(int id, CancellationToken cancellationToken)
        {
            var result = await _studentService.GetStudentByIdAsync(id, cancellationToken);
            
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";

            if (userRole == "Student" && result.UserId != userId)
            {
                return Forbid();
            }

            if (userRole == "Parent")
            {
                var parentId = await _parentService.GetParentIdByUserIdAsync(userId, cancellationToken);
                if (result.ParentId != parentId)
                {
                    return Forbid();
                }
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddStudent([FromBody] CreateStudentDTO dto, CancellationToken cancellationToken)
        {
            var result = await _studentService.AddStudentAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDTO dto, CancellationToken cancellationToken)
        {
            var result = await _studentService.UpdateStudentAsync(id, dto, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStudent(int id, CancellationToken cancellationToken)
        {
            await _studentService.DeleteStudentAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id}/enroll")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EnrollStudent(int id, [FromBody] EnrollStudentDTO dto, CancellationToken cancellationToken)
        {
            await _studentService.EnrollStudentAsync(id, dto, cancellationToken);
            return Ok();
        }

        [HttpPost("bulk-enroll")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkEnrollStudents([FromBody] BulkEnrollStudentsDTO dto, CancellationToken cancellationToken)
        {
            await _studentService.BulkEnrollStudentsAsync(dto, cancellationToken);
            return Ok();
        }
    }
}
