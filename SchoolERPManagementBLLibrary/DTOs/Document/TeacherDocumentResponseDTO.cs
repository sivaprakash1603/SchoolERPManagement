namespace SchoolERPManagementBLLibrary.DTOs.Document;

public record TeacherDocumentResponseDTO(int Id, int TeacherId, string DocumentType, string BlobUrl, DateTime? UploadedAt);
