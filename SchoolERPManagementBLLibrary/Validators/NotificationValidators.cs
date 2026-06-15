using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Notification;

namespace SchoolERPManagementBLLibrary.Validators;

public class SendNotificationValidator : AbstractValidator<SendNotificationDTO>
{
    public SendNotificationValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Message).NotEmpty();
        RuleFor(x => x.TargetUserIds).NotEmpty().WithMessage("Must provide Target User IDs.");
    }
}
