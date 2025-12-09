using FluentValidation;
using IAMS.Shared.QueryParams;
using IAMS.Domain.Enums;
using IAMS.Application.DTOs.Customer;

namespace IAMS.Application.Validators.Customer
{
    public class CreateCustomerValidator : AbstractValidator<CreateOrUpdateCustomerDto>
    {
        public CreateCustomerValidator()
        {
            // FirstName is always required (for individuals it's first name, for corporate it's company name)
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

            // LastName is only required for individual customers
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters")
                .When(x => x.Type == CustomerType.Individual);

            RuleFor(x => x.IdentificationNumber)
                .Length(11).WithMessage("Identification number must be exactly 11 digits")
                .Matches(@"^\d{11}$").WithMessage("Identification number must contain only digits")
                .When(x => !string.IsNullOrEmpty(x.IdentificationNumber));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .When(x => !string.IsNullOrEmpty(x.Email));

            // DateOfBirth is only validated for individual customers
            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past")
                .GreaterThan(DateTime.Today.AddYears(-120)).WithMessage("Date of birth cannot be more than 120 years ago")
                .When(x => x.DateOfBirth.HasValue && x.Type == CustomerType.Individual);
        }
    }
}