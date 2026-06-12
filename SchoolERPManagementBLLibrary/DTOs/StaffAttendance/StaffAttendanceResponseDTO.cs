namespace SchoolERPManagementBLLibrary.DTOs.StaffAttendance;

public record StaffAttendanceResponseDTO
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public string? Username { get; init; }
    public DateOnly Date { get; init; }
    public string Status { get; init; } = string.Empty;
    public string AttendanceType { get; init; } = string.Empty;
    public string? Remarks { get; init; }
}
