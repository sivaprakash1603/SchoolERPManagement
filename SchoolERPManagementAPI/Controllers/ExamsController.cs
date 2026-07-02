using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.Exam;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ExamsController : ControllerBase
    {
        private readonly IExamService _examService;

        public ExamsController(IExamService examService)
        {
            _examService = examService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateExam([FromBody] CreateExamDTO dto, CancellationToken cancellationToken)
        {
            var result = await _examService.CreateExamAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpPost("publish-result")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> PublishResult([FromBody] PublishResultDTO dto, CancellationToken cancellationToken)
        {
            var result = await _examService.PublishResultAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet("student/{studentId}/results")]
        [Authorize(Roles = "Admin,Teacher,Student,Parent")]
        public async Task<IActionResult> GetStudentResults(int studentId, CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";

            var result = await _examService.GetStudentResultsAsync(studentId, userId, userRole, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExams(CancellationToken cancellationToken)
        {
            var result = await _examService.GetAllExamsAsync(cancellationToken);
            return Ok(result);
        }

        [HttpPost("schedule")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateExamSchedule([FromBody] CreateExamScheduleDTO dto, CancellationToken cancellationToken)
        {
            var result = await _examService.CreateExamScheduleAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpPut("schedules/{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateExamSchedule(int id, [FromBody] UpdateExamScheduleDTO dto, CancellationToken cancellationToken)
        {
            var result = await _examService.UpdateExamScheduleAsync(id, dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{examId}/schedules")]
        public async Task<IActionResult> GetExamSchedules(int examId, CancellationToken cancellationToken)
        {
            var result = await _examService.GetExamSchedulesByExamIdAsync(examId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("results")]
        public async Task<IActionResult> GetExamResultsByClass([FromQuery] int examId, [FromQuery] int classId, [FromQuery] int subjectId, CancellationToken cancellationToken)
        {
            var result = await _examService.GetExamResultsByClassAsync(examId, classId, subjectId, cancellationToken);
            return Ok(result);
        }
    }
}
