namespace SchoolERPManagementBLLibrary.DTOs.Exam;

public record ExamResultResponseDTO(int Id, int ExamId, int SubjectId, int StudentId, decimal? Marks, string? UploadedCorrectedAnswerSheetUrl);
