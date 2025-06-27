using FluentValidation;
using IAMS.Application.DTOs.Policy;

namespace IAMS.Application.Validators.Policy
{
    public class CreatePolicyValidator : AbstractValidator<CreatePolicyDto>
    {
        public CreatePolicyValidator()
        {
            RuleFor(x => x.PolicyNumber)
                .NotEmpty().WithMessage("Policy number is required")
                .MaximumLength(50).WithMessage("Policy number must not exceed 50 characters");

            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Customer ID is required");

            RuleFor(x => x.InsuranceCompanyId)
                .GreaterThan(0).WithMessage("Insurance Company ID is required");

            RuleFor(x => x.PolicyTypeId)
                .GreaterThan(0).WithMessage("Policy Type ID is required");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required")
                .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be before or equal to end date");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

            RuleFor(x => x.PremiumAmount)
                .GreaterThan(0).WithMessage("Premium amount must be greater than 0")
                .LessThan(10000000).WithMessage("Premium amount cannot exceed 10,000,000");

            RuleFor(x => x.CommissionRate)
                .InclusiveBetween(0, 100).WithMessage("Commission rate must be between 0 and 100");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .Length(3).WithMessage("Currency must be 3 characters")
                .Must(BeValidCurrency).WithMessage("Invalid currency code");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters");
        }

        private bool BeValidCurrency(string currency)
        {
            var validCurrencies = new[] { "TRY", "USD", "EUR", "GBP" };
            return validCurrencies.Contains(currency?.ToUpper());
        }
    }
}