using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Student;

namespace SchoolERPManagementBLLibrary.Validators;

public class CreateStudentValidator : AbstractValidator<CreateStudentDTO>
{
    public CreateStudentValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage("A valid email is required.");
        RuleFor(x => x.ClassId).GreaterThan(0);
        RuleFor(x => x.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId.HasValue).WithMessage("Parent ID must be positive.");
    }
}

public class UpdateStudentValidator : AbstractValidator<UpdateStudentDTO>
{
    public UpdateStudentValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Name));
        RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId.HasValue);
    }
}
