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

            RuleFor(x => x.ContactEmail)
                .EmailAddress().WithMessage("Invalid email format")
                .When(x => !string.IsNullOrEmpty(x.ContactEmail));

            RuleFor(x => x.Website)
                .Must(BeValidUrl).WithMessage("Invalid website URL")
                .When(x => !string.IsNullOrEmpty(x.Website));
        }

        private bool BeValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}