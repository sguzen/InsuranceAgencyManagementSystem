using FluentValidation;
using IAMS.Application.Validators.Customer;

namespace IAMS.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
    {
        public CreateCustomerCommandValidator()
        {
            RuleFor(x => x.CustomerDto)
                .NotNull().WithMessage("Customer data is required.");

            RuleFor(x => x.CustomerDto)
                .SetValidator(new CreateCustomerValidator())
                .When(x => x.CustomerDto != null);
        }
    }
}
