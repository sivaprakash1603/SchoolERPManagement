namespace SchoolERPManagementBLLibrary.DTOs.Report;

public record StaffAttendanceReportDTO(
    int TotalWorkingDays,
    int PresentCount,
    int AbsentCount,
    double OverallPercentage,
    Dictionary<string, double> AttendanceByType
);
