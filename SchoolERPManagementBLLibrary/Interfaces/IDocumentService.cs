using Microsoft.AspNetCore.Http;
using SchoolERPManagementBLLibrary.DTOs.Document;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IDocumentService
{
    Task<StudentDocumentResponseDTO> UploadStudentDocumentAsync(IFormFile file, int studentId, CancellationToken cancellationToken);
    Task<TeacherDocumentResponseDTO> UploadTeacherDocumentAsync(IFormFile file, int teacherId, CancellationToken cancellationToken);
    Task DeleteDocumentAsync(string blobUrl, CancellationToken cancellationToken);
    Task VerifyDocumentAsync(VerifyDocumentDTO dto, int verifyingUserId, string userRole, CancellationToken cancellationToken);
}
