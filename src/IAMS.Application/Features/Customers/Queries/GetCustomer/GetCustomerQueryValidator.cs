using FluentValidation;

namespace IAMS.Application.Features.Customers.Queries.GetCustomer
{
    public class GetCustomerQueryValidator : AbstractValidator<GetCustomerQuery>
    {
        public GetCustomerQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Müşteri ID pozitif bir sayı olmalıdır");
        }
    }
}