namespace SchoolERPManagementBLLibrary.DTOs.Auth;

public record ResetPasswordDTO(string Email, string Token, string NewPassword);
