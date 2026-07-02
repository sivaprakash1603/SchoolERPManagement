using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.DTOs.Parent;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ParentsController : ControllerBase
    {
        private readonly IParentService _parentService;
        private readonly IPdfReportService _pdfReportService;

        public ParentsController(IParentService parentService, IPdfReportService pdfReportService)
        {
            _parentService = parentService;
            _pdfReportService = pdfReportService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAllParents([FromQuery] ParentQueryRequest request, CancellationToken cancellationToken)
        {
            var result = await _parentService.GetAllParentsAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetParentStats(CancellationToken cancellationToken)
        {
            var result = await _parentService.GetParentStatsAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("export/pdf")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> ExportParentsPdf([FromQuery] ParentQueryRequest request, CancellationToken cancellationToken)
        {
            request.PageNumber = 1;
            request.PageSize = int.MaxValue;
            var result = await _parentService.GetAllParentsAsync(request, cancellationToken);
            var pdfBytes = _pdfReportService.GenerateParentsPdf(result.Items.ToList());
            return File(pdfBytes, "application/pdf", "parents-directory.pdf");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetParentById(int id, CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";

            if (userRole == "Parent")
            {
                var parentId = await _parentService.GetParentIdByUserIdAsync(userId, cancellationToken);
                if (id != parentId)
                {
                    return Forbid();
                }
            }

            var result = await _parentService.GetParentByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}/children")]
        public async Task<IActionResult> GetChildren(int id, CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";

            if (userRole == "Parent")
            {
                var parentId = await _parentService.GetParentIdByUserIdAsync(userId, cancellationToken);
                if (id != parentId)
                {
                    return Forbid();
                }
            }

            var result = await _parentService.GetChildrenAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddParent([FromBody] CreateParentDTO dto, CancellationToken cancellationToken)
        {
            var result = await _parentService.AddParentAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateParent(int id, [FromBody] UpdateParentDTO dto, CancellationToken cancellationToken)
        {
            var result = await _parentService.UpdateParentAsync(id, dto, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteParent(int id, CancellationToken cancellationToken)
        {
            await _parentService.DeleteParentAsync(id, cancellationToken);
            return Ok(new { Message = "Parent deleted successfully." });
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetParentByUserId(int userId, CancellationToken cancellationToken)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";

            if (userRole != "Admin" && userRole != "Teacher" && currentUserId != userId)
            {
                return Forbid();
            }

            var parentId = await _parentService.GetParentIdByUserIdAsync(userId, cancellationToken);
            if (parentId == null || parentId == 0) return NotFound("Parent not found for this user.");

            var result = await _parentService.GetParentByIdAsync(parentId.Value, cancellationToken);
            return Ok(result);
        }
    }
}
