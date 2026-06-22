using System;

namespace SchoolERPManagementBLLibrary.DTOs.Homework
{
    public record HomeworkSubmissionDetailsDTO(
        int Id, 
        int HomeworkId, 
        int StudentId, 
        string StudentName, 
        string? UploadedFileUrl, 
        string? VerificationStatus, 
        decimal? Marks, 
        string? Remarks, 
        DateTime? SubmittedAt
    );
}
