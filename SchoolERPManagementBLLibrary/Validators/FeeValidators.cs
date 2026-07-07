using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Fee;

namespace SchoolERPManagementBLLibrary.Validators;

public class FeePaymentValidator : AbstractValidator<FeePaymentDTO>
{
    public FeePaymentValidator()
    {
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.FeeStructureId).GreaterThan(0);
        RuleFor(x => x.AmountPaid).GreaterThan(0).LessThanOrEqualTo(500000).WithMessage("Payment amount must be between 0 and 500,000.");
        RuleFor(x => x.PaymentMethod).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty().When(x => x.PaymentMethod?.ToLower() == "credit card" || x.PaymentMethod?.ToLower() == "stripe");
        RuleFor(x => x.PaymentDate)
            .Must(date => date == null || date <= System.DateTime.UtcNow)
            .WithMessage("Payment date cannot be in the future.");
    }
}

public class AddFeeStructureValidator : AbstractValidator<AddFeeStructureDTO>
{
    public AddFeeStructureValidator()
    {
        RuleFor(x => x.ClassId).GreaterThan(0);
        RuleFor(x => x.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.FeeName).NotEmpty();
        RuleFor(x => x.TotalAmount).GreaterThan(0);
        RuleFor(x => x.DueDate).NotEmpty().When(x => x.DueDate.HasValue).WithMessage("DueDate cannot be empty if provided.");
    }
}
