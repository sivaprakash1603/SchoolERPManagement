using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Auth;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Helpers;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class AuthService : IAuthService
{
    private readonly IRepository<int, User> _userRepository;
    private readonly IRepository<int, Role> _roleRepository;
    private readonly JwtTokenGenerator _tokenGenerator;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public AuthService(IRepository<int, User> userRepository, IRepository<int, Role> roleRepository, JwtTokenGenerator tokenGenerator, IEmailService emailService, IMapper mapper)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _tokenGenerator = tokenGenerator;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO dto, CancellationToken cancellationToken)
    {
        var user = await _userRepository.Query(true)
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Username == dto.Username, cancellationToken);

        if (user is null || !PasswordHasher.Verify(dto.Password, user.Passwordhash))
        {
            throw new BusinessRuleException("Invalid username/email or password.");
        }

        var response = _mapper.Map<AuthResponseDTO>(user);
        return response with { AccessToken = _tokenGenerator.GenerateToken(user, user.Role) };
    }

    public async Task<string> ForgotPasswordAsync(ForgotPasswordDTO dto, CancellationToken cancellationToken)
    {
        var user = await _userRepository.Query(false)
            .FirstOrDefaultAsync(x => x.Email == dto.Email, cancellationToken);

        if (user is null)
        {
            // For security reasons, don't throw exception if email is not found, just return a fake token or empty string
            // But since this is an internal API for now, we can return empty or throw. Let's throw for ease of testing.
            throw new EntityNotFoundException("User", dto.Email);
        }

        user.Resettoken = Guid.NewGuid().ToString();
        user.Resettokenexpiry = DateTime.UtcNow.AddHours(1);

        await _userRepository.UpdateAsync(user, save: true, ct: cancellationToken);

        string emailBody = $@"
        <h2>Password Reset Request</h2>
        <p>Dear {user.Username},</p>
        <p>You have requested to reset your password. Please use the following reset token to proceed:</p>
        <p><strong>{user.Resettoken}</strong></p>
        <p>This token is valid for 1 hour.</p>
        <p>If you did not request a password reset, please ignore this email.</p>";

        try
        {
            await _emailService.SendEmailAsync(dto.Email, "Password Reset Request", emailBody, cancellationToken);
        }
        catch
        {
            // Log email sending failure
        }

        return user.Resettoken;
    }

    public async Task ResetPasswordAsync(ResetPasswordDTO dto, CancellationToken cancellationToken)
    {
        var user = await _userRepository.Query(false)
            .FirstOrDefaultAsync(x => x.Email == dto.Email, cancellationToken);

        if (user is null || user.Resettoken != dto.Token || user.Resettokenexpiry < DateTime.UtcNow)
        {
            throw new BusinessRuleException("Invalid or expired reset token.");
        }

        user.Passwordhash = PasswordHasher.Hash(dto.NewPassword);
        user.Resettoken = null;
        user.Resettokenexpiry = null;

        await _userRepository.UpdateAsync(user, save: true, ct: cancellationToken);
    }

    public async Task ChangePasswordAsync(ChangePasswordDTO dto, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(dto.UserId);
        if (user is null)
        {
            throw new EntityNotFoundException("User", dto.UserId.ToString());
        }

        if (!PasswordHasher.Verify(dto.CurrentPassword, user.Passwordhash))
        {
            throw new BusinessRuleException("Current password is incorrect.");
        }

        user.Passwordhash = PasswordHasher.Hash(dto.NewPassword);
        await _userRepository.UpdateAsync(user, save: true, ct: cancellationToken);
    }
}
