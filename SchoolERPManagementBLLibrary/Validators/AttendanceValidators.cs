using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Attendance;

namespace SchoolERPManagementBLLibrary.Validators;

public class MarkAttendanceValidator : AbstractValidator<MarkAttendanceDTO>
{
    public MarkAttendanceValidator()
    {
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.Date).LessThanOrEqualTo(System.DateOnly.FromDateTime(System.DateTime.Today)).WithMessage("Attendance date cannot be in the future.");
        RuleFor(x => x.Status).NotEmpty().Must(status => status.ToLower() == "present" || status.ToLower() == "absent" || status.ToLower() == "late").WithMessage("Status must be Present, Absent, or Late.");
    }
}
