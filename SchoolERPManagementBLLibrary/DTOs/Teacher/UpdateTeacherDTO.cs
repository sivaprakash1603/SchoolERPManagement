namespace SchoolERPManagementBLLibrary.DTOs.Teacher;

public record UpdateTeacherDTO(string FirstName, string LastName, string? Phonenumber, string? Qualifications, int? SubjectSpecialtyId = null);
