namespace SchoolERPManagementBLLibrary.DTOs.Timetable;

public record CreateTimetableDTO(int ClassId, int SubjectId, int TeacherId, string DayOfWeek, TimeOnly StartTime, TimeOnly EndTime, string? RoomNo);
