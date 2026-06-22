using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.Homework;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HomeworkController : ControllerBase
    {
        private readonly IHomeworkService _homeworkService;
        private readonly ITeacherService _teacherService;

        public HomeworkController(IHomeworkService homeworkService, ITeacherService teacherService)
        {
            _homeworkService = homeworkService;
            _teacherService = teacherService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateHomework([FromForm] CreateHomeworkDTO dto, CancellationToken cancellationToken)
        {
            if (User.IsInRole("Teacher"))
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                if (int.TryParse(userIdString, out int userId))
                {
                    bool isAssigned = await _teacherService.VerifyTeacherAssignmentAsync(userId, dto.TeacherId, dto.ClassId, dto.SubjectId, cancellationToken);
                    if (!isAssigned)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    return Unauthorized();
                }
            }

            var result = await _homeworkService.CreateHomeworkAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpPost("submit")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> SubmitHomework([FromForm] HomeworkSubmissionDTO dto, CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            int? userId = int.TryParse(userIdStr, out var id) ? id : null;
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "";

            var result = await _homeworkService.SubmitHomeworkAsync(dto, userId, userRole, cancellationToken);
            return Ok(result);
        }

        [HttpPost("evaluate")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> EvaluateHomework([FromBody] EvaluateHomeworkDTO dto, CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            int? userId = int.TryParse(userIdStr, out var id) ? id : null;
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "";

            var result = await _homeworkService.EvaluateHomeworkAsync(dto, userId, userRole, cancellationToken);
            return Ok(result);
        }

        [HttpGet("class/{classId}")]
        [Authorize(Roles = "Admin,Teacher,Student,Parent")]
        public async Task<IActionResult> GetHomeworks(int classId, [FromQuery] int? subjectId, CancellationToken cancellationToken)
        {
            var result = await _homeworkService.GetHomeworksAsync(classId, subjectId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Teacher,Student,Parent")]
        public async Task<IActionResult> GetHomeworksByUser(int userId, CancellationToken cancellationToken)
        {
            // Security check to ensure a user only fetches their own homework unless they are an admin
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
            
            if (role != "Admin" && currentUserIdStr != userId.ToString())
            {
                return Forbid();
            }

            var result = await _homeworkService.GetHomeworksByUserIdAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Teacher,Student,Parent")]
        public async Task<IActionResult> GetHomeworksByStudentId(int studentId, CancellationToken cancellationToken)
        {
            var result = await _homeworkService.GetHomeworksByStudentIdAsync(studentId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{homeworkId}/submissions")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetSubmissions(int homeworkId, CancellationToken cancellationToken)
        {
            var result = await _homeworkService.GetSubmissionsByHomeworkIdAsync(homeworkId, cancellationToken);
            return Ok(result);
        }
    }
}
