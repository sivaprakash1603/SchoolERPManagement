using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Student;

namespace SchoolERPManagementBLLibrary.Validators;

public class CreateStudentDTOValidator : AbstractValidator<CreateStudentDTO>
{
    public CreateStudentDTOValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ClassId).GreaterThan(0);
        RuleFor(x => x.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Parents).Must(p => p == null || p.All(s => s.ParentId > 0 && !string.IsNullOrEmpty(s.Relation))).WithMessage("All parents must have a valid ID and relation.");
    }
}

public class UpdateStudentDTOValidator : AbstractValidator<UpdateStudentDTO>
{
    public UpdateStudentDTOValidator()
    {
        RuleFor(x => x.Name).MaximumLength(150).When(x => !string.IsNullOrEmpty(x.Name));
        RuleFor(x => x.Gender).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Gender));
        RuleFor(x => x.Parents).Must(p => p == null || p.All(s => s.ParentId > 0 && !string.IsNullOrEmpty(s.Relation))).WithMessage("All parents must have a valid ID and relation.");
    }
}
