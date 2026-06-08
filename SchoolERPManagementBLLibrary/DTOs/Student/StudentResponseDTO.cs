namespace SchoolERPManagementBLLibrary.DTOs.Student;

public record StudentResponseDTO(int Id, int UserId, string RegNo, string Name, int? ParentId, string? GeneratedPassword = null);
