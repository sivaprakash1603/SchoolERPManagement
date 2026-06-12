namespace SchoolERPManagementBLLibrary.DTOs.Report;

public record StudentAttendanceReportDTO(
    int TotalClasses,
    int PresentCount,
    int AbsentCount,
    double OverallPercentage,
    Dictionary<string, double> AttendanceByClass
);
