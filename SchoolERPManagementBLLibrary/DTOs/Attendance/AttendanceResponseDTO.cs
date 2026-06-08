namespace SchoolERPManagementBLLibrary.DTOs.Attendance;

public record AttendanceResponseDTO(int Id, int StudentId, DateOnly Date, string Status, int? MarkedByTeacherId, string? Remarks);
