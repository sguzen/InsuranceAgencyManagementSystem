using FluentValidation;
using IAMS.Application.DTOs.Policy;

namespace IAMS.Application.Validators.Policy
{
    public class UpdatePolicyValidator : AbstractValidator<UpdatePolicyDto>
    {
        public UpdatePolicyValidator()
        {
            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

            RuleFor(x => x.PremiumAmount)
                .GreaterThan(0).WithMessage("Premium amount must be greater than 0");

            RuleFor(x => x.CommissionRate)
                .InclusiveBetween(0, 100).WithMessage("Commission rate must be between 0 and 100");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters");
        }
    }
}