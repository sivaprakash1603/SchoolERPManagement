using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Exam;

namespace SchoolERPManagementBLLibrary.Validators;

public class CreateExamValidator : AbstractValidator<CreateExamDTO>
{
    public CreateExamValidator()
    {
        RuleFor(x => x.Examname).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AcademicyearId).GreaterThan(0).When(x => x.AcademicyearId.HasValue);
    }
}

public class PublishResultValidator : AbstractValidator<PublishResultDTO>
{
    public PublishResultValidator()
    {
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.ExamId).GreaterThan(0);
        RuleFor(x => x.SubjectId).GreaterThan(0);
        RuleFor(x => x.Marks).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100).When(x => x.Marks.HasValue).WithMessage("Marks must be between 0 and 100.");
    }
}
