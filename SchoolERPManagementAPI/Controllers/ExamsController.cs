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
            var result = await _examService.GetStudentResultsAsync(studentId, cancellationToken);
            return Ok(result);
        }
    }
}
