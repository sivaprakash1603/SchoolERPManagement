using System;

namespace SchoolERPManagementBLLibrary.DTOs.Exam
{
    public record ExamScheduleResponseDTO(
        int Id,
        int ExamId,
        int SubjectId,
        string SubjectName,
        int ClassId,
        string ClassName,
        string ClassSection,
        DateOnly ExamDate,
        int DurationMinutes,
        string? Session
    );
}
