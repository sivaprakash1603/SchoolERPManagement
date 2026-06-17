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

        public StudentsController(IStudentService studentService, IParentService parentService)
        {
            _studentService = studentService;
            _parentService = parentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAllStudents(CancellationToken cancellationToken)
        {
            var result = await _studentService.GetAllStudentsAsync(cancellationToken);
            return Ok(result);
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
    }
}
