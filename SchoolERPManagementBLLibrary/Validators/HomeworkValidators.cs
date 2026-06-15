using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Homework;

namespace SchoolERPManagementBLLibrary.Validators;

public class CreateHomeworkValidator : AbstractValidator<CreateHomeworkDTO>
{
    public CreateHomeworkValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(System.DateOnly.FromDateTime(System.DateTime.Today)).WithMessage("Due date cannot be in the past.");
        RuleFor(x => x.ClassId).GreaterThan(0);
        RuleFor(x => x.SubjectId).GreaterThan(0);
        RuleFor(x => x.TeacherId).GreaterThan(0);
    }
}

public class EvaluateHomeworkValidator : AbstractValidator<EvaluateHomeworkDTO>
{
    public EvaluateHomeworkValidator()
    {
        RuleFor(x => x.HomeworkSubmissionId).GreaterThan(0);
        RuleFor(x => x.Marks).GreaterThanOrEqualTo(0).When(x => x.Marks.HasValue);
    }
}

public class HomeworkSubmissionValidator : AbstractValidator<HomeworkSubmissionDTO>
{
    public HomeworkSubmissionValidator()
    {
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.HomeworkId).GreaterThan(0);
        RuleFor(x => x.UploadedFile).NotNull().WithMessage("Uploaded file is required.");
    }
}
