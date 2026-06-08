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

        public TeachersController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTeachers(CancellationToken cancellationToken)
        {
            var result = await _teacherService.GetAllTeachersAsync(cancellationToken);
            return Ok(result);
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
    }
}
