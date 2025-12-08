using FluentValidation;
using IAMS.Application.DTOs.Policy;
using IAMS.Shared.QueryParams;

namespace IAMS.Application.Validators.Policy
{
    public class CreatePolicyValidator : AbstractValidator<CreatePolicyDto>
    {
        public CreatePolicyValidator()
        {
            // Policy number is optional - will be auto-generated if not provided
            RuleFor(x => x.PolicyNumber)
                .MaximumLength(50).WithMessage("Policy number must not exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.PolicyNumber));

            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Customer ID is required");

            RuleFor(x => x.InsuranceCompanyId)
                .GreaterThan(0).WithMessage("Insurance Company ID is required");

            RuleFor(x => x.PolicyTypeId)
                .GreaterThan(0).WithMessage("Policy Type ID is required");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

            RuleFor(x => x.PremiumAmount)
                .GreaterThan(0).WithMessage("Premium amount must be greater than 0");

            RuleFor(x => x.CommissionRate)
                .InclusiveBetween(0, 100).WithMessage("Commission rate must be between 0 and 100");
        }
    }
}