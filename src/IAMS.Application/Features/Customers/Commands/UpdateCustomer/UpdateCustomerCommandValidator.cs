using FluentValidation;

namespace IAMS.Application.Features.Customers.Commands.UpdateCustomer
{
    public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
    {
        public UpdateCustomerCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Müşteri ID geçerli olmalıdır");

            RuleFor(x => x.CustomerDto)
                .NotNull()
                .WithMessage("Müşteri bilgileri gereklidir");

            RuleFor(x => x.CustomerDto)
                .SetValidator(new IAMS.Application.Validators.Customer.UpdateCustomerValidator())
                .When(x => x.CustomerDto != null);
        }
    }
}