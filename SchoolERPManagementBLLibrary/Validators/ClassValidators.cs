using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Class;

namespace SchoolERPManagementBLLibrary.Validators;

public class CreateClassValidator : AbstractValidator<CreateClassDTO>
{
    public CreateClassValidator()
    {
        RuleFor(x => x.Classname).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Section).NotEmpty().MaximumLength(10);
    }
}
