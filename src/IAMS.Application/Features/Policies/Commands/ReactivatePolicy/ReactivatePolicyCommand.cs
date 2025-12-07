using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Commands.ReactivatePolicy
{
    public class ReactivatePolicyCommand : IRequest<Result<PolicyDto>>
    {
        public int Id { get; set; }
    }
}