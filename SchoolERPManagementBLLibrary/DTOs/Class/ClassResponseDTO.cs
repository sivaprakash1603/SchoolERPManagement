namespace SchoolERPManagementBLLibrary.DTOs.Class;

public record ClassResponseDTO(int Id, string Classname, string? Section, int? ClassteacherId, int? AcademicyearId, int StudentCount = 0);
