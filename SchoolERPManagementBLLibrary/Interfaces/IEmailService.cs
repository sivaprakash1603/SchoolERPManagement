namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken);
    Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, byte[] attachmentData, string attachmentName, CancellationToken cancellationToken);
}
