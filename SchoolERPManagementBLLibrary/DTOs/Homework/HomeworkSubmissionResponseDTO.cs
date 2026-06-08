namespace SchoolERPManagementBLLibrary.DTOs.Homework;

public record HomeworkSubmissionResponseDTO(int Id, int HomeworkId, int StudentId, string? UploadedFileUrl, string? VerificationStatus, decimal? Marks, string? Remarks, DateTime? SubmittedAt);
