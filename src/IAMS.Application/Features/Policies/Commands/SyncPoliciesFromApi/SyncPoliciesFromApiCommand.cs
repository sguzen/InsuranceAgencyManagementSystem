using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;

namespace IAMS.Application.Features.Policies.Commands.SyncPoliciesFromApi
{
    public class SyncPoliciesFromApiCommand : IRequest<Result<PolicyImportResultDto>>
    {
        public int ImportConfigurationId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string UserId { get; set; }

        public SyncPoliciesFromApiCommand(int importConfigurationId, string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            ImportConfigurationId = importConfigurationId;
            UserId = userId;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
