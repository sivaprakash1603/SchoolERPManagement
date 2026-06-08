namespace SchoolERPManagementBLLibrary.DTOs.Student;

public record CreateStudentDTO(string Email, string Name, int ClassId, int AcademicYearId, int? ParentId);
