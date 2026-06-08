namespace SchoolERPManagementBLLibrary.DTOs.StaffAttendance;

public record StaffAttendanceResponseDTO(
    int Id,
    int UserId,
    string? Username,
    DateOnly Date,
    string Status,
    string AttendanceType,
    string? Remarks
);
