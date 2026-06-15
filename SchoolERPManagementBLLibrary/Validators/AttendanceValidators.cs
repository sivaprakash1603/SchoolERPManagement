using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Attendance;

namespace SchoolERPManagementBLLibrary.Validators;

public class MarkAttendanceValidator : AbstractValidator<MarkAttendanceDTO>
{
    public MarkAttendanceValidator()
    {
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.Date).LessThanOrEqualTo(System.DateOnly.FromDateTime(System.DateTime.Today)).WithMessage("Attendance date cannot be in the future.");
        RuleFor(x => x.Status).NotEmpty().Must(status => status == "Present" || status == "Absent" || status == "Late").WithMessage("Status must be Present, Absent, or Late.");
    }
}
