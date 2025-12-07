using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.ValidateEmail
{
    public record ValidateEmailQuery(string Email, int? ExcludeCustomerId = null) : IRequest<Result<bool>>;
}