using FluentValidation;
using IAMS.Application.DTOs.InsuranceCompany;

namespace IAMS.Application.Validators.InsuranceCompany
{
    public class CreateInsuranceCompanyValidator : AbstractValidator<CreateInsuranceCompanyDto>
    {
        public CreateInsuranceCompanyValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Company name is required")
                .MaximumLength(200).WithMessage("Company name must not exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

            RuleFor(x => x.ContactEmail)
                .EmailAddress().WithMessage("Email format is invalid")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.ContactEmail));

            RuleFor(x => x.ContactPhone)
                .MaximumLength(20).WithMessage("Phone must not exceed 20 characters");

            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("Address must not exceed 500 characters");

            RuleFor(x => x.Website)
                .MaximumLength(255).WithMessage("Website must not exceed 255 characters");
        }
    }
}