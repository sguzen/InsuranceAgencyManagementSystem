using FluentValidation;
using IAMS.Application.DTOs.Customer;

namespace IAMS.Application.Validators.Customer
{
    public class CreateCustomerValidator : AbstractValidator<CreateOrUpdateCustomerDto>
    {
        public CreateCustomerValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

            RuleFor(x => x.IdentificationNo)
                .Length(11).WithMessage("TC number must be 10 digits")
                .Matches(@"^\d{10}$").WithMessage("KKTC number must contain only digits")
                .When(x => !string.IsNullOrEmpty(x.IdentificationNo));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past")
                .GreaterThan(DateTime.Today.AddYears(-120)).WithMessage("Date of birth cannot be more than 120 years ago")
                .When(x => x.DateOfBirth.HasValue);
        }
    }
}