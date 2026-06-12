using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.Fee;
using SchoolERPManagementBLLibrary.Interfaces;

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

        public FeesController(IFeeService feeService, IStudentService studentService, IParentService parentService)
        {
            _feeService = feeService;
            _studentService = studentService;
            _parentService = parentService;
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
                var student = await _studentService.GetStudentByIdAsync(studentId, cancellationToken);
                return student.ParentId == myParentId;
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
        [Authorize(Roles = "Admin,Parent,Student")]
        public async Task<IActionResult> GetFeeDetails(int studentId, CancellationToken cancellationToken)
        {
            if (!await IsAuthorizedForStudentAsync(studentId, cancellationToken)) return Forbid();

            var result = await _feeService.GetFeeDetailsAsync(studentId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("student/{studentId}/history")]
        [Authorize(Roles = "Admin,Parent,Student")]
        public async Task<IActionResult> GetPaymentHistory(int studentId, CancellationToken cancellationToken)
        {
            if (!await IsAuthorizedForStudentAsync(studentId, cancellationToken)) return Forbid();

            var result = await _feeService.GetPaymentHistoryAsync(studentId, cancellationToken);
            return Ok(result);
        }

        [HttpPost("create-checkout-session")]
        [Authorize(Roles = "Admin,Parent,Student")]
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

        [HttpPost("structure")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddFeeStructure([FromBody] AddFeeStructureDTO dto, CancellationToken cancellationToken)
        {
            var result = await _feeService.AddFeeStructureAsync(dto, cancellationToken);
            return Ok(result);
        }
    }
}
