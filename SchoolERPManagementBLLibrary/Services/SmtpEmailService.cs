using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SchoolERPManagementBLLibrary.Configuration;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementBLLibrary.Services;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ISmtpClient _smtpClient;

    public SmtpEmailService(IOptions<SmtpSettings> smtpSettings, ISmtpClient? smtpClient = null)
    {
        _smtpSettings = smtpSettings.Value;
        _smtpClient = smtpClient ?? new SmtpClient();
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            HtmlBody = body,
            TextBody = body
        };

        message.Body = builder.ToMessageBody();

        try
        {
            // Connect using STARTTLS if available, otherwise fallback depending on the port
            await _smtpClient.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.Auto, cancellationToken);
            
            if (!string.IsNullOrEmpty(_smtpSettings.Username) && !string.IsNullOrEmpty(_smtpSettings.Password))
            {
                await _smtpClient.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password, cancellationToken);
            }

            await _smtpClient.SendAsync(message, cancellationToken);
        }
        finally
        {
            await _smtpClient.DisconnectAsync(true, cancellationToken);
            
            // If we created it internally, dispose it. Since MailKit's SmtpClient implements IDisposable
            if (_smtpClient is IDisposable disposable && _smtpClient.GetType() == typeof(SmtpClient))
            {
                disposable.Dispose();
            }
        }
    }

    public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, byte[] attachmentData, string attachmentName, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            HtmlBody = body,
            TextBody = body
        };

        builder.Attachments.Add(attachmentName, attachmentData, new ContentType("application", "pdf"));

        message.Body = builder.ToMessageBody();

        try
        {
            await _smtpClient.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.Auto, cancellationToken);
            
            if (!string.IsNullOrEmpty(_smtpSettings.Username) && !string.IsNullOrEmpty(_smtpSettings.Password))
            {
                await _smtpClient.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password, cancellationToken);
            }

            await _smtpClient.SendAsync(message, cancellationToken);
        }
        finally
        {
            await _smtpClient.DisconnectAsync(true, cancellationToken);
            
            if (_smtpClient is IDisposable disposable && _smtpClient.GetType() == typeof(SmtpClient))
            {
                disposable.Dispose();
            }
        }
    }
}
