namespace SchoolERPManagementBLLibrary.DTOs.Document;

public record TeacherDocumentResponseDTO(int Id, int TeacherId, string DocumentName, string DocumentType, string BlobUrl, DateTime? UploadedAt, string Status);
