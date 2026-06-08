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
            var result = await _homeworkService.SubmitHomeworkAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpPost("evaluate")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> EvaluateHomework([FromBody] EvaluateHomeworkDTO dto, CancellationToken cancellationToken)
        {
            var result = await _homeworkService.EvaluateHomeworkAsync(dto, cancellationToken);
            return Ok(result);
        }
    }
}
