using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Parent;

namespace SchoolERPManagementBLLibrary.Validators;

public class CreateParentValidator : AbstractValidator<CreateParentDTO>
{
    public CreateParentValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50).Matches(@"^[a-zA-Z\s\-]+$").WithMessage("First name can only contain letters, spaces, and hyphens.");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50).Matches(@"^[a-zA-Z\s\-]+$").WithMessage("Last name can only contain letters, spaces, and hyphens.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage("A valid email is required.");
        RuleFor(x => x.Phonenumber).NotEmpty().MaximumLength(20).Matches(@"^[6-9]\d{9}$").WithMessage("Phone number must be exactly 10 digits and start with 6, 7, 8, or 9.");
    }
}

public class UpdateParentValidator : AbstractValidator<UpdateParentDTO>
{
    public UpdateParentValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50).Matches(@"^[a-zA-Z\s\-]+$").WithMessage("First name can only contain letters, spaces, and hyphens.");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50).Matches(@"^[a-zA-Z\s\-]+$").WithMessage("Last name can only contain letters, spaces, and hyphens.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage("A valid email is required.");
        RuleFor(x => x.Phonenumber).NotEmpty().MaximumLength(20).Matches(@"^[6-9]\d{9}$").WithMessage("Phone number must be exactly 10 digits and start with 6, 7, 8, or 9.");
    }
}
