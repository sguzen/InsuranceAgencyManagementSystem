using IAMS.Domain.Entities;
using IAMS.Shared.DTOs.Policy;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Commands.SyncPoliciesFromMySql
{
    public class SyncPoliciesFromMySqlCommand : IRequest<Result<PolicyImportResultDto>>
    {
        /// <summary>
        /// Start date for the import range
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for the import range
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Insurance company ID in the tenant database
        /// </summary>
        public int InsuranceCompanyId { get; set; }

        /// <summary>
        /// User performing the import
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Optional: pre-built ImportConfiguration from master DB credentials.
        /// When provided, the handler uses this instead of looking up from tenant DB.
        /// This allows the background import pipeline to work without requiring
        /// a manually-created ImportConfiguration record in the tenant database.
        /// </summary>
        public ImportConfiguration? ExternalConfiguration { get; set; }

        public SyncPoliciesFromMySqlCommand(
            DateTime startDate,
            DateTime endDate,
            int insuranceCompanyId,
            string userId)
        {
            StartDate = startDate;
            EndDate = endDate;
            InsuranceCompanyId = insuranceCompanyId;
            UserId = userId;
        }
    }
}
