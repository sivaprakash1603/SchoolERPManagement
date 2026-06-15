using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Document;

namespace SchoolERPManagementBLLibrary.Validators;

public class VerifyDocumentValidator : AbstractValidator<VerifyDocumentDTO>
{
    public VerifyDocumentValidator()
    {
        RuleFor(x => x.Status).NotEmpty().Must(status => status == "Verified" || status == "Rejected" || status == "Pending").WithMessage("Verification status must be Verified, Rejected, or Pending.");
    }
}
