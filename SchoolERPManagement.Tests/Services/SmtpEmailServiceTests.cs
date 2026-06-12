using FluentAssertions;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;
using SchoolERPManagementBLLibrary.Configuration;
using SchoolERPManagementBLLibrary.Services;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class SmtpEmailServiceTests
{
    private readonly Mock<ISmtpClient> _smtpClientMock;
    private readonly IOptions<SmtpSettings> _smtpSettingsOptions;
    private readonly SmtpEmailService _emailService;

    public SmtpEmailServiceTests()
    {
        _smtpClientMock = new Mock<ISmtpClient>();
        
        var smtpSettings = new SmtpSettings
        {
            Host = "smtp.test.com",
            Port = 587,
            Username = "user@test.com",
            Password = "password",
            SenderName = "Test Sender",
            SenderEmail = "sender@test.com"
        };
        
        _smtpSettingsOptions = Options.Create(smtpSettings);
        
        _emailService = new SmtpEmailService(_smtpSettingsOptions, _smtpClientMock.Object,
            new Moq.Mock<AutoMapper.IMapper>().Object
        );
    }

    [Fact]
    public async Task SendEmailAsync_ValidData_ShouldConnectAuthenticateAndSend()
    {
        
        _smtpClientMock.Setup(c => c.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SecureSocketOptions>(), It.IsAny<CancellationToken>()))
                       .Returns(Task.CompletedTask);
        
        _smtpClientMock.Setup(c => c.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .Returns(Task.CompletedTask);
                       
        _smtpClientMock.Setup(c => c.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
                       .ReturnsAsync("MockResponse");
                       
        _smtpClientMock.Setup(c => c.DisconnectAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                       .Returns(Task.CompletedTask);

        
        await _emailService.SendEmailAsync("recipient@test.com", "Test Subject", "Test Body", CancellationToken.None);

        
        _smtpClientMock.Verify(c => c.ConnectAsync("smtp.test.com", 587, SecureSocketOptions.Auto, It.IsAny<CancellationToken>()), Times.Once);
        _smtpClientMock.Verify(c => c.AuthenticateAsync("user@test.com", "password", It.IsAny<CancellationToken>()), Times.Once);
        _smtpClientMock.Verify(c => c.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()), Times.Once);
        _smtpClientMock.Verify(c => c.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }
}
