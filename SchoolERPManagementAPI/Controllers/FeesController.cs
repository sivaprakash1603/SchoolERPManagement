using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Fee;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FeesController : ControllerBase
    {
        private readonly IFeeService _feeService;
        private readonly IStudentService _studentService;
        private readonly IParentService _parentService;
        private readonly IRepository<int, SchoolERPManagementModelLibrary.Models.Teacher> _teacherRepository;
        private readonly IRepository<int, SchoolERPManagementModelLibrary.Models.Timetable> _timetableRepository;
        private readonly IRepository<int, SchoolERPManagementModelLibrary.Models.Teachersubject> _teacherSubjectRepository;

        public FeesController(
            IFeeService feeService, 
            IStudentService studentService, 
            IParentService parentService,
            IRepository<int, SchoolERPManagementModelLibrary.Models.Teacher> teacherRepository,
            IRepository<int, SchoolERPManagementModelLibrary.Models.Timetable> timetableRepository,
            IRepository<int, SchoolERPManagementModelLibrary.Models.Teachersubject> teacherSubjectRepository)
        {
            _feeService = feeService;
            _studentService = studentService;
            _parentService = parentService;
            _teacherRepository = teacherRepository;
            _timetableRepository = timetableRepository;
            _teacherSubjectRepository = teacherSubjectRepository;
        }

        private async Task<bool> IsAuthorizedForStudentAsync(int studentId, CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";

            if (userRole == "Admin") return true;

            if (userRole == "Student")
            {
                var myStudentId = await _studentService.GetStudentIdByUserIdAsync(userId, cancellationToken);
                return studentId == myStudentId;
            }

            if (userRole == "Parent")
            {
                var myParentId = await _parentService.GetParentIdByUserIdAsync(userId, cancellationToken);
                if (myParentId == null) return false;
                var children = await _parentService.GetChildrenAsync(myParentId.Value, cancellationToken);
                return children.Any(c => c.StudentId == studentId);
            }

            if (userRole == "Teacher")
            {
                // Verify if teacher teaches any classes/subjects that contain this student.
                // We check if student is enrolled in any class that this teacher teaches.
                var student = await _studentService.GetStudentByIdAsync(studentId, cancellationToken);
                if (student == null) return false;

                // Let's resolve the teacher's ID
                var teacher = await _teacherRepository.Query(true)
                    .FirstOrDefaultAsync(t => t.Userid == userId, cancellationToken);

                if (teacher == null) return false;

                // Check if there is any timetable slot or teachersubject slot mapping the teacher to the student's class
                var isTeached = await _timetableRepository.Query(true)
                    .AnyAsync(t => t.Teacherid == teacher.Id && t.Classid == student.ClassId, cancellationToken)
                    || await _teacherSubjectRepository.Query(true)
                    .AnyAsync(ts => ts.Teacherid == teacher.Id && ts.Classid == student.ClassId, cancellationToken);

                return isTeached;
            }

            return false;
        }

        [HttpPost("pay")]
        [Authorize(Roles = "Admin,Parent,Student")]
        public async Task<IActionResult> PayFees([FromBody] FeePaymentDTO dto, CancellationToken cancellationToken)
        {
            if (!await IsAuthorizedForStudentAsync(dto.StudentId, cancellationToken)) return Forbid();

            var result = await _feeService.PayFeesAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet("student/{studentId}/summary")]
        [Authorize(Roles = "Admin,Teacher,Parent,Student")]
        public async Task<IActionResult> GetFeeDetails(int studentId, CancellationToken cancellationToken)
        {
            if (!await IsAuthorizedForStudentAsync(studentId, cancellationToken)) return Forbid();

            var result = await _feeService.GetFeeDetailsAsync(studentId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("student/{studentId}/history")]
        [Authorize(Roles = "Admin,Teacher,Parent,Student")]
        public async Task<IActionResult> GetPaymentHistory(int studentId, CancellationToken cancellationToken)
        {
            if (!await IsAuthorizedForStudentAsync(studentId, cancellationToken)) return Forbid();

            var result = await _feeService.GetPaymentHistoryAsync(studentId, cancellationToken);
            return Ok(result);
        }

        [HttpPost("create-checkout-session")]
        [Authorize(Roles = "Admin,Teacher,Parent,Student")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionDTO dto, CancellationToken cancellationToken)
        {
            if (!await IsAuthorizedForStudentAsync(dto.StudentId, cancellationToken)) return Forbid();

            var sessionUrl = await _feeService.CreateStripeCheckoutSessionAsync(dto, cancellationToken);
            return Ok(new { Url = sessionUrl });
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = HttpContext.Request.Headers["Stripe-Signature"].ToString();

            try
            {
                await _feeService.HandleStripeWebhookAsync(json, signature, cancellationToken);
                return Ok();
            }
            catch (Exception)
            {
                
                return BadRequest();
            }
        }

        [HttpGet("checkout-session/{sessionId}")]
        [Authorize(Roles = "Admin,Teacher,Parent,Student")]
        public async Task<IActionResult> GetCheckoutSessionDetails(string sessionId, CancellationToken cancellationToken)
        {
            var result = await _feeService.GetCheckoutSessionDetailsAsync(sessionId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("receipt/{transactionId}")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadReceipt(string transactionId, CancellationToken cancellationToken)
        {
            try
            {
                var pdfBytes = await _feeService.GenerateReceiptPdfAsync(transactionId, cancellationToken);
                return File(pdfBytes, "application/pdf", $"Receipt_{transactionId}.pdf");
            }
            catch (SchoolERPManagementBLLibrary.Exceptions.EntityNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("structure")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddFeeStructure([FromBody] AddFeeStructureDTO dto, CancellationToken cancellationToken)
        {
            var result = await _feeService.AddFeeStructureAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet("class/{classId}/summaries")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetClassFeeSummaries(int classId, [FromQuery] int academicYearId, CancellationToken cancellationToken)
        {
            var result = await _feeService.GetClassFeeSummariesAsync(classId, academicYearId, cancellationToken);
            return Ok(result);
        }
    }
}
