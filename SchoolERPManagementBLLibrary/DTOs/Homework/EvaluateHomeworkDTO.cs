namespace SchoolERPManagementBLLibrary.DTOs.Homework;

public record EvaluateHomeworkDTO(int HomeworkSubmissionId, decimal? Marks, string? Remarks, string? VerificationStatus);
