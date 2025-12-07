using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerByIdentificationNo
{
    public record GetCustomerByIdentificationNoQuery(string IdentificationNo) : IRequest<Result<CustomerDto>>;
}