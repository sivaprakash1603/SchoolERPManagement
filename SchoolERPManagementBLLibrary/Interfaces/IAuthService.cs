using SchoolERPManagementBLLibrary.DTOs.Auth;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDTO> LoginAsync(LoginRequestDTO dto, CancellationToken cancellationToken);
    Task<string> ForgotPasswordAsync(ForgotPasswordDTO dto, CancellationToken cancellationToken);
    Task ResetPasswordAsync(ResetPasswordDTO dto, CancellationToken cancellationToken);
    Task ChangePasswordAsync(ChangePasswordDTO dto, CancellationToken cancellationToken);
}
