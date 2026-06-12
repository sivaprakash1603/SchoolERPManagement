using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Teacher;

namespace SchoolERPManagementBLLibrary.Validators;

public class CreateTeacherValidator : AbstractValidator<CreateTeacherDTO>
{
    public CreateTeacherValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phonenumber).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Phonenumber));
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0).WithMessage("Salary cannot be negative.");
    }
}
