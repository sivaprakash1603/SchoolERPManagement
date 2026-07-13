using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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
    [Fact]
    public async Task SendEmailAsync_WithAuth_ShouldAuthenticateAndSend()
    {
        var settings = new SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            Username = "user",
            Password = "password",
            SenderEmail = "sender@example.com",
            SenderName = "Sender"
        };
        var optionsMock = new Mock<IOptions<SmtpSettings>>();
        optionsMock.Setup(o => o.Value).Returns(settings);

        var smtpClientMock = new Mock<ISmtpClient>();
        var service = new SmtpEmailService(optionsMock.Object, smtpClientMock.Object);

        await service.SendEmailAsync("to@example.com", "Subject", "Body", CancellationToken.None);

        smtpClientMock.Verify(c => c.ConnectAsync("smtp.example.com", 587, SecureSocketOptions.Auto, It.IsAny<CancellationToken>()), Times.Once);
        smtpClientMock.Verify(c => c.AuthenticateAsync("user", "password", It.IsAny<CancellationToken>()), Times.Once);
        smtpClientMock.Verify(c => c.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<MailKit.ITransferProgress>()), Times.Once);
        smtpClientMock.Verify(c => c.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WithoutAuth_ShouldNotAuthenticateAndSend()
    {
        var settings = new SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 25,
            Username = "",
            Password = "",
            SenderEmail = "sender@example.com",
            SenderName = "Sender"
        };
        var optionsMock = new Mock<IOptions<SmtpSettings>>();
        optionsMock.Setup(o => o.Value).Returns(settings);

        var smtpClientMock = new Mock<ISmtpClient>();
        var service = new SmtpEmailService(optionsMock.Object, smtpClientMock.Object);

        await service.SendEmailAsync("to@example.com", "Subject", "Body", CancellationToken.None);

        smtpClientMock.Verify(c => c.ConnectAsync("smtp.example.com", 25, SecureSocketOptions.Auto, It.IsAny<CancellationToken>()), Times.Once);
        smtpClientMock.Verify(c => c.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        smtpClientMock.Verify(c => c.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<MailKit.ITransferProgress>()), Times.Once);
        smtpClientMock.Verify(c => c.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailWithAttachmentAsync_WithAuth_ShouldAuthenticateAndSend()
    {
        var settings = new SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            Username = "user",
            Password = "password",
            SenderEmail = "sender@example.com",
            SenderName = "Sender"
        };
        var optionsMock = new Mock<IOptions<SmtpSettings>>();
        optionsMock.Setup(o => o.Value).Returns(settings);

        var smtpClientMock = new Mock<ISmtpClient>();
        var service = new SmtpEmailService(optionsMock.Object, smtpClientMock.Object);

        var attachmentData = new byte[] { 1, 2, 3 };

        await service.SendEmailWithAttachmentAsync("to@example.com", "Subject", "Body", attachmentData, "file.pdf", CancellationToken.None);

        smtpClientMock.Verify(c => c.ConnectAsync("smtp.example.com", 587, SecureSocketOptions.Auto, It.IsAny<CancellationToken>()), Times.Once);
        smtpClientMock.Verify(c => c.AuthenticateAsync("user", "password", It.IsAny<CancellationToken>()), Times.Once);
        smtpClientMock.Verify(c => c.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<MailKit.ITransferProgress>()), Times.Once);
        smtpClientMock.Verify(c => c.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailWithAttachmentAsync_WithoutAuth_ShouldNotAuthenticateAndSend()
    {
        var settings = new SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 25,
            Username = "",
            Password = "",
            SenderEmail = "sender@example.com",
            SenderName = "Sender"
        };
        var optionsMock = new Mock<IOptions<SmtpSettings>>();
        optionsMock.Setup(o => o.Value).Returns(settings);

        var smtpClientMock = new Mock<ISmtpClient>();
        var service = new SmtpEmailService(optionsMock.Object, smtpClientMock.Object);

        var attachmentData = new byte[] { 1, 2, 3 };

        await service.SendEmailWithAttachmentAsync("to@example.com", "Subject", "Body", attachmentData, "file.pdf", CancellationToken.None);

        smtpClientMock.Verify(c => c.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<MailKit.ITransferProgress>()), Times.Once);
        smtpClientMock.Verify(c => c.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task SendEmailAsync_RealClient_ShouldDisposeOnException()
    {
        var settings = new SmtpSettings
        {
            Host = "invalid.example.com", // Will fail to connect
            Port = 25,
            SenderName = "Test",
            SenderEmail = "test@example.com"
        };
        var optionsMock = new Mock<IOptions<SmtpSettings>>();
        optionsMock.Setup(o => o.Value).Returns(settings);

        var service = new SmtpEmailService(optionsMock.Object);

        Func<Task> action = async () => await service.SendEmailAsync("to@example.com", "Subject", "Body", CancellationToken.None);

        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SendEmailWithAttachmentAsync_RealClient_ShouldDisposeOnException()
    {
        var settings = new SmtpSettings
        {
            Host = "invalid.example.com",
            Port = 25,
            SenderName = "Test",
            SenderEmail = "test@example.com"
        };
        var optionsMock = new Mock<IOptions<SmtpSettings>>();
        optionsMock.Setup(o => o.Value).Returns(settings);

        var service = new SmtpEmailService(optionsMock.Object);

        Func<Task> action = async () => await service.SendEmailWithAttachmentAsync("to@example.com", "Subject", "Body", new byte[] { 1, 2, 3 }, "file.pdf", CancellationToken.None);

        await action.Should().ThrowAsync<Exception>();
    }
}
