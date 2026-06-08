namespace SchoolERPManagementBLLibrary.DTOs.Document;

public record StudentDocumentResponseDTO(int Id, int StudentId, string DocumentType, string BlobUrl, DateTime? UploadedAt);
