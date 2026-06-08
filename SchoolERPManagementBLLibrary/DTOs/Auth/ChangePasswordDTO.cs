namespace SchoolERPManagementBLLibrary.DTOs.Auth;

public record ChangePasswordDTO(int UserId, string CurrentPassword, string NewPassword);
