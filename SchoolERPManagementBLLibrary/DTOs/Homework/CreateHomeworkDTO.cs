using Microsoft.AspNetCore.Http;

namespace SchoolERPManagementBLLibrary.DTOs.Homework;

public record CreateHomeworkDTO(int SubjectId, int TeacherId, int ClassId, string Title, string? Description, IFormFile? Attachment, DateOnly DueDate);
