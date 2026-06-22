using System;

namespace SchoolERPManagementBLLibrary.DTOs.Exam
{
    public record CreateExamScheduleDTO(
        int ExamId,
        int SubjectId,
        int ClassId,
        DateOnly ExamDate,
        int DurationMinutes,
        string? Session
    );
}
