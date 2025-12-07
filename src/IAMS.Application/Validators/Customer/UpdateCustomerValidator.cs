using FluentValidation;
using IAMS.Shared.QueryParams;
using IAMS.Domain.Enums;

namespace IAMS.Application.Validators.Customer
{
    public class UpdateCustomerValidator : AbstractValidator<CreateOrUpdateCustomerDto>
    {
        public UpdateCustomerValidator()
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

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email format is invalid")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

            RuleFor(x => x.MobilePhoneNumber)
                .NotEmpty().WithMessage("Phone is required")
                .MaximumLength(20).WithMessage("Phone must not exceed 20 characters");

            //RuleFor(x => x.Address)
            //    .NotEmpty().WithMessage("Address is required")
            //    .MaximumLength(500).WithMessage("Address must not exceed 500 characters");

            // DateOfBirth is only required for individual customers
            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required")
                .Must(BeAValidAge).WithMessage("Customer must be at least 18 years old")
                .When(x => x.Type == CustomerType.Individual);
        }

        private bool BeAValidAge(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
                return false;

            var age = DateTime.Today.Year - dateOfBirth.Value.Year;
            if (dateOfBirth.Value.Date > DateTime.Today.AddYears(-age)) age--;
            return age >= 18;
        }
    }
}