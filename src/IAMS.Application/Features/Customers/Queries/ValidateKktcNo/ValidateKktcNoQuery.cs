using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.ValidateIdentificationNo
{
    public record ValidateIdentificationNoQuery(string IdentificationNo, int? ExcludeCustomerId = null) : IRequest<Result<bool>>;
}