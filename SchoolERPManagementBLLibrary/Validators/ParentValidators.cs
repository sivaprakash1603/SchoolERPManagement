using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Parent;

namespace SchoolERPManagementBLLibrary.Validators;

public class CreateParentValidator : AbstractValidator<CreateParentDTO>
{
    public CreateParentValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage("A valid email is required.");
        RuleFor(x => x.Phonenumber).NotEmpty().MaximumLength(20).Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format.");
    }
}
