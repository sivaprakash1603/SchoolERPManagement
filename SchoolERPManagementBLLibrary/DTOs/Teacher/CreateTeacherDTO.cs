namespace SchoolERPManagementBLLibrary.DTOs.Teacher;

public record CreateTeacherDTO(string Email, string Name, string? Phonenumber, string? Qualifications, int? SubjectSpecialtyId = null);
