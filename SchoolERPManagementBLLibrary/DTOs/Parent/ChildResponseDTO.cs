namespace SchoolERPManagementBLLibrary.DTOs.Parent;

public record ChildResponseDTO(int StudentId, int UserId, string RegNo, string FirstName, string LastName, int? ClassId, string? ClassName, string? Section);
