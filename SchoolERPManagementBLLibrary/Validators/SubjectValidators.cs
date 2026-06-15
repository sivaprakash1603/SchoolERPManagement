using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Subject;

namespace SchoolERPManagementBLLibrary.Validators;

public class CreateSubjectValidator : AbstractValidator<CreateSubjectDTO>
{
    public CreateSubjectValidator()
    {
        RuleFor(x => x.SubjectName).NotEmpty().MaximumLength(100);
    }
}
