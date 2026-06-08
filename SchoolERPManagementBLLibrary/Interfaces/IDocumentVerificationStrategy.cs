using SchoolERPManagementBLLibrary.DTOs.Document;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IDocumentVerificationStrategy
{
    bool CanHandle(string documentType);
    Task VerifyAsync(VerifyDocumentDTO dto, int verifyingUserId, string userRole, CancellationToken cancellationToken);
}
