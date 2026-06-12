namespace SchoolERPManagementBLLibrary.DTOs.Report;

public record ExamPerformanceReportDTO(
    int ExamId,
    string ExamName,
    int TotalStudentsAppeared,
    decimal OverallAverageMarks,
    Dictionary<string, decimal> AverageMarksBySubject
);
