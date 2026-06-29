namespace SchoolERPManagementBLLibrary.DTOs.Timetable;

public record TeacherRequirementDTO(
    int SubjectId,
    string SubjectName,
    int TotalClassesTakingSubject,
    int RequiredTeachers,
    int AvailableTeachers,
    string Status
);
