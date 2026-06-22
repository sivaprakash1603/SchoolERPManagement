using System;

namespace SchoolERPManagementBLLibrary.DTOs.Document
{
    public record PendingDocumentDTO(
        int Id,
        string DocumentName,
        string DocumentType,
        string BlobUrl,
        DateTime? UploadedAt,
        int OwnerId,
        string OwnerName,
        string OwnerType,
        string OwnerIdentifier // RegNo for student, Username for teacher
    );
}
