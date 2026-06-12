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
        result.RoleName.Should().Be("Admin");
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
}
