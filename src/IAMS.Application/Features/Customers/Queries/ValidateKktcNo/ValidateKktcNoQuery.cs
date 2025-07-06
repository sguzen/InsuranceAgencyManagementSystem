using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.ValidateKktcNo
{
    public record ValidateKktcNoQuery(string KktcNo, int? ExcludeCustomerId = null) : IRequest<Result<bool>>;
}