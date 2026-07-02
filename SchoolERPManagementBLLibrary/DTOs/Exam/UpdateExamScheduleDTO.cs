using System;

namespace SchoolERPManagementBLLibrary.DTOs.Exam
{
    public record UpdateExamScheduleDTO(
        int ClassId,
        int SubjectId,
        DateOnly ExamDate,
        int DurationMinutes,
        string? Session
    );
}
