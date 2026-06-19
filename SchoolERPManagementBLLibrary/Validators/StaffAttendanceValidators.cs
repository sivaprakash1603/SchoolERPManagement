using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.StaffAttendance;

namespace SchoolERPManagementBLLibrary.Validators;

public class StaffAttendanceValidator : AbstractValidator<StaffAttendanceRequestDTO>
{
    public StaffAttendanceValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Date).LessThanOrEqualTo(System.DateOnly.FromDateTime(System.DateTime.Today)).WithMessage("Attendance date cannot be in the future.");
        RuleFor(x => x.Status).NotEmpty().Must(status => status.ToLower() == "present" || status.ToLower() == "absent" || status.ToLower() == "late" || status.ToLower() == "onleave").WithMessage("Status must be Present, Absent, Late, or OnLeave.");
    }
}
