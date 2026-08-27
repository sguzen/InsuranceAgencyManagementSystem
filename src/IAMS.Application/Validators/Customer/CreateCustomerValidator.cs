using FluentValidation;
using IAMS.Shared.QueryParams;
using IAMS.Domain.Enums;
using IAMS.Shared.DTOs.Customer;

namespace IAMS.Application.Validators.Customer
{
    public class CreateCustomerValidator : AbstractValidator<CreateOrUpdateCustomerDto>
    {
        public CreateCustomerValidator()
        {
            // FirstName is always required (for individuals it's first name, for corporate it's company name)
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("First name cannot be empty or whitespace only")
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

            // LastName is only required for individual customers
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Last name cannot be empty or whitespace only")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters")
                .When(x => x.Type == CustomerType.Individual);

            // No format/length rule for the identification number: old KKTC kimlik numbers
            // were 6 digits (later extended), passports vary — only the column size applies.
            RuleFor(x => x.IdentificationNumber)
                .MaximumLength(50).WithMessage("Identification number must not exceed 50 characters")
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