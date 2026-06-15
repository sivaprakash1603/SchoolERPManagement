using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Teacher;

namespace SchoolERPManagementBLLibrary.Validators;

public class CreateTeacherValidator : AbstractValidator<CreateTeacherDTO>
{
    public CreateTeacherValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage("A valid email is required.");
        RuleFor(x => x.Phonenumber).MaximumLength(20).Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format.").When(x => !string.IsNullOrEmpty(x.Phonenumber));
    }
}

public class AssignTeacherSubjectValidator : AbstractValidator<AssignTeacherSubjectDTO>
{
    public AssignTeacherSubjectValidator()
    {
        RuleFor(x => x.TeacherId).GreaterThan(0);
        RuleFor(x => x.SubjectId).GreaterThan(0);
        RuleFor(x => x.ClassId).GreaterThan(0);
    }
}
