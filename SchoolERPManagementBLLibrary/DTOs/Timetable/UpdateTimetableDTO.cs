namespace SchoolERPManagementBLLibrary.DTOs.Timetable;

public record UpdateTimetableDTO(
    int SubjectId,
    int TeacherId,
    string? RoomNo
);
