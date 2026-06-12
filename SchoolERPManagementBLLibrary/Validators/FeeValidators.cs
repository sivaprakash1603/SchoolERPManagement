using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Fee;

namespace SchoolERPManagementBLLibrary.Validators;

public class FeePaymentValidator : AbstractValidator<FeePaymentDTO>
{
    public FeePaymentValidator()
    {
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.FeeStructureId).GreaterThan(0);
        RuleFor(x => x.AmountPaid).GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
        RuleFor(x => x.PaymentMethod).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty().When(x => x.PaymentMethod == "Credit Card" || x.PaymentMethod == "Stripe");
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
    }
}
