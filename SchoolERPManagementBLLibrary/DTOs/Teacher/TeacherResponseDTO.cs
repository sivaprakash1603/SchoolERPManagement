namespace SchoolERPManagementBLLibrary.DTOs.Teacher;

public record TeacherResponseDTO(
    int Id, 
    int UserId, 
    string FirstName, string LastName, 
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
    string? SubjectSpecialtyName = null,
    IEnumerable<SimpleTeacherAssignmentDTO>? Assignments = null
);

public record SimpleTeacherAssignmentDTO(
    int ClassId,
    int SubjectId
);
