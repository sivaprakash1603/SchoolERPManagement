namespace SchoolERPManagementBLLibrary.DTOs.Document;

public record StudentDocumentResponseDTO(int Id, int StudentId, string DocumentName, string DocumentType, string BlobUrl, DateTime? UploadedAt);
