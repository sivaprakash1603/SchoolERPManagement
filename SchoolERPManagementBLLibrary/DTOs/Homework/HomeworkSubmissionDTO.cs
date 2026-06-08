using Microsoft.AspNetCore.Http;

namespace SchoolERPManagementBLLibrary.DTOs.Homework;

public record HomeworkSubmissionDTO(int HomeworkId, int StudentId, IFormFile? UploadedFile);
