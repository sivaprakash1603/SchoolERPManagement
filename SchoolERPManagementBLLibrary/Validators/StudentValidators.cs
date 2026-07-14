using FluentValidation;
using SchoolERPManagementBLLibrary.DTOs.Student;
using System;
using System.Linq;

namespace SchoolERPManagementBLLibrary.Validators;

public class CreateStudentDTOValidator : AbstractValidator<CreateStudentDTO>
{
    public CreateStudentDTOValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(75);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(75);
        RuleFor(x => x.ClassId).GreaterThan(0);
        RuleFor(x => x.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Parents).Must(p => p == null || p.All(s => s.ParentId > 0 && !string.IsNullOrEmpty(s.Relation))).WithMessage("All parents must have a valid ID and relation.");
        RuleFor(x => x.Admissiondate)
            .Must(date => date == null || date <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Admission date cannot be in the future.");
        RuleFor(x => x.Dateofbirth)
            .Must(date => date == null || date <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth cannot be in the future.");
    }
}

public class UpdateStudentDTOValidator : AbstractValidator<UpdateStudentDTO>
{
    public UpdateStudentDTOValidator()
    {
        RuleFor(x => x.FirstName).MaximumLength(75).When(x => !string.IsNullOrEmpty(x.FirstName));
        RuleFor(x => x.LastName).MaximumLength(75).When(x => !string.IsNullOrEmpty(x.LastName));
        RuleFor(x => x.Gender).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Gender));
        RuleFor(x => x.Parents).Must(p => p == null || p.All(s => s.ParentId > 0 && !string.IsNullOrEmpty(s.Relation))).WithMessage("All parents must have a valid ID and relation.");
        RuleFor(x => x.Admissiondate)
            .Must(date => date == null || date <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Admission date cannot be in the future.");
        RuleFor(x => x.Dateofbirth)
            .Must(date => date == null || date <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth cannot be in the future.");
    }
}
