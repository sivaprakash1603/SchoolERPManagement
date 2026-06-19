namespace SchoolERPManagementBLLibrary.DTOs.Homework;

public record HomeworkResponseDTO(int Id, int SubjectId, int TeacherId, int ClassId, string Title, string? Description, string? AttachmentUrl, DateTime? CreatedAt, DateOnly DueDate, HomeworkSubmissionResponseDTO? Submission = null);
