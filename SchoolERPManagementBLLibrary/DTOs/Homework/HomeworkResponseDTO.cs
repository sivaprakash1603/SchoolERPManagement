namespace SchoolERPManagementBLLibrary.DTOs.Homework;

public record HomeworkResponseDTO(int Id, int SubjectId, string? SubjectName, int TeacherId, string? TeacherName, int ClassId, string Title, string? Description, string? AttachmentUrl, DateTime? CreatedAt, DateOnly DueDate, HomeworkSubmissionResponseDTO? Submission = null);
