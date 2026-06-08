namespace SchoolERPManagementBLLibrary.DTOs.Parent;

public record ChildResponseDTO(int StudentId, int UserId, string RegNo, string Name, int? ClassId, string? ClassName, string? Section);
