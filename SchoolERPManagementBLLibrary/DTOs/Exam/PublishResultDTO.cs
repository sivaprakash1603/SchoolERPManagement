namespace SchoolERPManagementBLLibrary.DTOs.Exam;

public record PublishResultDTO(int ExamId, int SubjectId, int StudentId, decimal? Marks, string? UploadedCorrectedAnswerSheetUrl);
