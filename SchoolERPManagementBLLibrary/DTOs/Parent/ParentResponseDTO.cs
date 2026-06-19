namespace SchoolERPManagementBLLibrary.DTOs.Parent;

public record ParentResponseDTO(int Id, int UserId, string Name, string? Relation, string? Phonenumber, string? Email = null, string? GeneratedPassword = null, string? Username = null);
