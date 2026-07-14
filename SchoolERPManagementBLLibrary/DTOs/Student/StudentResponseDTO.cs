namespace SchoolERPManagementBLLibrary.DTOs.Student;

public record StudentResponseDTO(int Id, int UserId, string RegNo, string FirstName, string LastName, int? ParentId, string? GeneratedPassword = null, int? ClassId = null, string? ProfilePhotoUrl = null, string? Email = null);
