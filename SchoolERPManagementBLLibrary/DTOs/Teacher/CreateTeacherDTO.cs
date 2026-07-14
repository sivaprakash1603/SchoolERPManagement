namespace SchoolERPManagementBLLibrary.DTOs.Teacher;

public record CreateTeacherDTO(string Email, string FirstName, string LastName, string? Phonenumber, string? Qualifications, int? SubjectSpecialtyId = null);
