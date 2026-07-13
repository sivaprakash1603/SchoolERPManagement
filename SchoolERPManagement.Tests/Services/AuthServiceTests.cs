using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Auth;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Helpers;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IRepository<int, User>> _userRepositoryMock;
    private readonly Mock<IRepository<int, Role>> _roleRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IRepository<int, User>>();
        _roleRepositoryMock = new Mock<IRepository<int, Role>>();
        _configurationMock = new Mock<IConfiguration>();
        _emailServiceMock = new Mock<IEmailService>();

        _configurationMock.Setup(c => c["Jwt:Key"]).Returns("ThisIsAVerySecretKeyForTestingPurposesOnly123!");
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

        var tokenGenerator = new JwtTokenGenerator(_configurationMock.Object);
        _authService = new AuthService(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            tokenGenerator,
            _emailServiceMock.Object,
            SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()
        );
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldReturnToken()
    {
        
        var password = "Password123";
        var user = new User
        {
            Id = 1,
            Username = "jdoe",
            Passwordhash = PasswordHasher.Hash(password),
            Isactive = true,
            Role = new Role { Rolename = "Admin" }
        };

        var usersList = new List<User> { user };
        _userRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(usersList.BuildMockDbSet().Object);

        var dto = new LoginRequestDTO("jdoe", password);

        
        var result = await _authService.LoginAsync(dto, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ShouldThrowBusinessRuleException()
    {
        
        var user = new User
        {
            Username = "jdoe",
            Passwordhash = PasswordHasher.Hash("correctpassword"),
            Isactive = true
        };

        var usersList = new List<User> { user };
        _userRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(usersList.BuildMockDbSet().Object);

        var dto = new LoginRequestDTO("jdoe", "wrongpassword");

        
        Func<Task> action = async () => await _authService.LoginAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("Invalid username/email or password.");
    }

    [Fact]
    public async Task LoginAsync_DeactivatedUser_ShouldThrowBusinessRuleException()
    {
        var user = new User
        {
            Username = "jdoe",
            Passwordhash = PasswordHasher.Hash("Password123"),
            Isactive = false
        };

        var usersList = new List<User> { user };
        _userRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(usersList.BuildMockDbSet().Object);

        var dto = new LoginRequestDTO("jdoe", "Password123");

        Func<Task> action = async () => await _authService.LoginAsync(dto, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("Your account has been deactivated.");
    }

    [Fact]
    public async Task ForgotPasswordAsync_UserExists_ShouldGenerateTokenAndSendEmail()
    {
        
        var user = new User { Email = "test@example.com", Resettoken = null };
        var usersList = new List<User> { user };
        _userRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(usersList.BuildMockDbSet().Object);

        var dto = new ForgotPasswordDTO("test@example.com");

        
        await _authService.ForgotPasswordAsync(dto, CancellationToken.None);

        
        user.Resettoken.Should().NotBeNullOrEmpty();
        _userRepositoryMock.Verify(r => r.UpdateAsync(user, true, It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendEmailAsync(user.Email, "Password Reset Request", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_UserNotFound_ShouldThrowEntityNotFoundException()
    {
        _userRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User>().BuildMockDbSet().Object);
        var dto = new ForgotPasswordDTO("test@example.com");

        Func<Task> action = async () => await _authService.ForgotPasswordAsync(dto, CancellationToken.None);

        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ForgotPasswordAsync_EmailFails_ShouldStillGenerateToken()
    {
        var user = new User { Email = "test@example.com", Resettoken = null };
        var usersList = new List<User> { user };
        _userRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(usersList.BuildMockDbSet().Object);
        _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                         .ThrowsAsync(new Exception("Email failed"));

        var dto = new ForgotPasswordDTO("test@example.com");

        await _authService.ForgotPasswordAsync(dto, CancellationToken.None);

        user.Resettoken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidToken_ShouldUpdatePassword()
    {
        
        var token = "valid-token";
        var user = new User
        {
            Email = "test@example.com",
            Resettoken = token,
            Resettokenexpiry = DateTime.UtcNow.AddMinutes(30)
        };
        var usersList = new List<User> { user };
        _userRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(usersList.BuildMockDbSet().Object);

        var dto = new ResetPasswordDTO("test@example.com", token, "newPassword123");

        
        await _authService.ResetPasswordAsync(dto, CancellationToken.None);

        
        user.Resettoken.Should().BeNull();
        user.Resettokenexpiry.Should().BeNull();
        PasswordHasher.Verify("newPassword123", user.Passwordhash).Should().BeTrue();
        _userRepositoryMock.Verify(r => r.UpdateAsync(user, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ExpiredToken_ShouldThrowBusinessRuleException()
    {
        
        var token = "expired-token";
        var user = new User
        {
            Email = "test@example.com",
            Resettoken = token,
            Resettokenexpiry = DateTime.UtcNow.AddMinutes(-10) 
        };
        var usersList = new List<User> { user };
        _userRepositoryMock.Setup(r => r.Query(false)).Returns(usersList.BuildMockDbSet().Object);

        var dto = new ResetPasswordDTO("test@example.com", token, "newPassword123");

        
        Func<Task> action = async () => await _authService.ResetPasswordAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("Invalid or expired reset token.");
    }

    [Fact]
    public async Task ResetPasswordAsync_UserNotFound_ShouldThrowBusinessRuleException()
    {
        _userRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<User>().BuildMockDbSet().Object);
        var dto = new ResetPasswordDTO("test@example.com", "token", "newPassword123");

        Func<Task> action = async () => await _authService.ResetPasswordAsync(dto, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("Invalid or expired reset token.");
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidData_ShouldUpdatePassword()
    {
        var user = new User { Id = 1, Passwordhash = PasswordHasher.Hash("oldPassword123") };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var dto = new ChangePasswordDTO(1, "oldPassword123", "newPassword123");

        await _authService.ChangePasswordAsync(dto, CancellationToken.None);

        PasswordHasher.Verify("newPassword123", user.Passwordhash).Should().BeTrue();
        _userRepositoryMock.Verify(r => r.UpdateAsync(user, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_UserNotFound_ShouldThrowEntityNotFoundException()
    {
        _userRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User)null);
        var dto = new ChangePasswordDTO(99, "oldPassword", "newPassword");

        Func<Task> action = async () => await _authService.ChangePasswordAsync(dto, CancellationToken.None);

        await action.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task ChangePasswordAsync_InvalidCurrentPassword_ShouldThrowBusinessRuleException()
    {
        var user = new User { Id = 1, Passwordhash = PasswordHasher.Hash("oldPassword123") };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var dto = new ChangePasswordDTO(1, "wrongPassword", "newPassword123");

        Func<Task> action = async () => await _authService.ChangePasswordAsync(dto, CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("Current password is incorrect.");
    }
}
