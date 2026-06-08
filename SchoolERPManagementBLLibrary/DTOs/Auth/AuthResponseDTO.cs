namespace SchoolERPManagementBLLibrary.DTOs.Auth;

public record AuthResponseDTO(int UserId, string Username, string Email, int RoleId, string RoleName, string AccessToken);
