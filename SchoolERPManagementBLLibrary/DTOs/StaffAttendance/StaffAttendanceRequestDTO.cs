using System.ComponentModel.DataAnnotations;

namespace SchoolERPManagementBLLibrary.DTOs.StaffAttendance;

public record StaffAttendanceRequestDTO(
    [Required] int UserId,
    [Required] DateOnly Date,
    [Required] string Status,
    [Required] string AttendanceType,
    string? Remarks
);
