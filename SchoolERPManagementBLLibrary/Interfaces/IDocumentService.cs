using Microsoft.AspNetCore.Http;
using SchoolERPManagementBLLibrary.DTOs.Document;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IDocumentService
{
    Task<StudentDocumentResponseDTO> UploadStudentDocumentAsync(IFormFile file, int studentId, string? documentName, string? userRole = null, CancellationToken cancellationToken = default);
    Task<TeacherDocumentResponseDTO> UploadTeacherDocumentAsync(IFormFile file, int teacherId, string? documentName, CancellationToken cancellationToken);
    Task DeleteDocumentAsync(string blobUrl, CancellationToken cancellationToken);
    Task VerifyDocumentAsync(VerifyDocumentDTO dto, int verifyingUserId, string userRole, CancellationToken cancellationToken);
    Task<IReadOnlyList<StudentDocumentResponseDTO>> GetStudentDocumentsAsync(int studentId, int userId, string userRole, CancellationToken cancellationToken);
    Task<IReadOnlyList<TeacherDocumentResponseDTO>> GetTeacherDocumentsAsync(int teacherId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PendingDocumentDTO>> GetPendingDocumentsAsync(CancellationToken cancellationToken);
}
