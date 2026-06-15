using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Timetable;

namespace SchoolERPManagementBLLibrary.Validators;

public class CreateTimetableValidator : AbstractValidator<CreateTimetableDTO>
{
    public CreateTimetableValidator()
    {
        RuleFor(x => x.ClassId).GreaterThan(0);
        RuleFor(x => x.SubjectId).GreaterThan(0);
        RuleFor(x => x.TeacherId).GreaterThan(0);
        RuleFor(x => x.DayOfWeek).NotEmpty().Must(d => d == "Monday" || d == "Tuesday" || d == "Wednesday" || d == "Thursday" || d == "Friday" || d == "Saturday" || d == "Sunday").WithMessage("Invalid day of week.");
        RuleFor(x => x.StartTime).LessThan(x => x.EndTime).WithMessage("Start time must be before end time.");
    }
}
