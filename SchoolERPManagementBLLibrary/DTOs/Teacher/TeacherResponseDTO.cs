namespace SchoolERPManagementBLLibrary.DTOs.Teacher;

public record TeacherResponseDTO(
    int Id, 
    int UserId, 
    string Name, 
    string? Phonenumber, 
    DateOnly? Joiningdate, 
    string? Qualifications, 
    string? GeneratedPassword = null, 
    string? Username = null, 
    string? Classname = null,
    string? Section = null,
    string? Email = null,
    string? ProfilePhotoUrl = null,
    int AssignmentsCount = 0,
    int? SubjectSpecialtyId = null,
    string? SubjectSpecialtyName = null
);
