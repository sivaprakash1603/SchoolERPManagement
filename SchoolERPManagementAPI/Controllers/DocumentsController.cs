using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost("student/{studentId}")]
        [Authorize(Roles = "Admin,Parent,Student")]
        public async Task<IActionResult> UploadStudentDocument(IFormFile file, int studentId, [FromForm] string? documentName, CancellationToken cancellationToken)
        {
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            var result = await _documentService.UploadStudentDocumentAsync(file, studentId, documentName, userRole, cancellationToken);
            return Ok(result);
        }

        [HttpPost("teacher/{teacherId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UploadTeacherDocument(IFormFile file, int teacherId, [FromForm] string? documentName, CancellationToken cancellationToken)
        {
            var result = await _documentService.UploadTeacherDocumentAsync(file, teacherId, documentName, cancellationToken);
            return Ok(result);
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDocument([FromQuery] string blobUrl, CancellationToken cancellationToken)
        {
            await _documentService.DeleteDocumentAsync(blobUrl, cancellationToken);
            return NoContent();
        }

        [HttpPut("verify")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> VerifyDocument([FromBody] SchoolERPManagementBLLibrary.DTOs.Document.VerifyDocumentDTO dto, CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            
            await _documentService.VerifyDocumentAsync(dto, userId, userRole, cancellationToken);
            return Ok(new { Message = "Document verified successfully." });
        }

        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Parent,Student,Teacher")]
        public async Task<IActionResult> GetStudentDocuments(int studentId, CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";

            var result = await _documentService.GetStudentDocumentsAsync(studentId, userId, userRole, cancellationToken);
            return Ok(result);
        }

        [HttpGet("teacher/{teacherId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetTeacherDocuments(int teacherId, CancellationToken cancellationToken)
        {
            var result = await _documentService.GetTeacherDocumentsAsync(teacherId, cancellationToken);
            return Ok(result);
        }
    }
}
