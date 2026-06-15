using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Asset;

namespace SchoolERPManagementBLLibrary.Validators;

public class CreateAssetValidator : AbstractValidator<CreateAssetDTO>
{
    public CreateAssetValidator()
    {
        RuleFor(x => x.Assetname).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AssettypeId).GreaterThan(0).When(x => x.AssettypeId.HasValue);
        RuleFor(x => x.Purchasedate).LessThanOrEqualTo(System.DateOnly.FromDateTime(System.DateTime.Today)).When(x => x.Purchasedate.HasValue).WithMessage("Purchase date cannot be in the future.");
    }
}

public class AssetIssueValidator : AbstractValidator<AssetIssueDTO>
{
    public AssetIssueValidator()
    {
        RuleFor(x => x.AssetId).GreaterThan(0);
        RuleFor(x => x.Status).NotEmpty();
        RuleFor(x => x.Report).NotEmpty();
    }
}
