namespace SchoolERPManagementBLLibrary.DTOs.Attendance;

public record MarkAttendanceDTO(int StudentId, DateOnly Date, string Status, int? MarkedByTeacherId, string? Remarks);
