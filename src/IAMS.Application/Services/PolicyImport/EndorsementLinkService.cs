using IAMS.Application.Interfaces.Services;
using IAMS.Domain.Entities;
using IAMS.Shared.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.PolicyImport
{
    public class EndorsementLinkService : IEndorsementLinkService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EndorsementLinkService> _logger;

        public EndorsementLinkService(IUnitOfWork unitOfWork, ILogger<EndorsementLinkService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<int> LinkOrphanEndorsementsAsync(Policy originalPolicy, CancellationToken cancellationToken = default)
        {
            if (originalPolicy.InnerCode != "000" || string.IsNullOrEmpty(originalPolicy.PolicyNumber))
            {
                return 0;
            }

            var orphans = await _unitOfWork.Policies.AsQueryable()
                .Where(p => !p.IsDeleted &&
                            p.InsuranceCompanyId == originalPolicy.InsuranceCompanyId &&
                            p.PolicyNumber == originalPolicy.PolicyNumber &&
                            p.InnerCode != "000" &&
                            p.OriginalPolicyId == null)
                .ToListAsync(cancellationToken);

            foreach (var orphan in orphans)
            {
                // Navigation property, so linking also works when the original itself is
                // still unsaved in the current batch (EF resolves the FK on SaveChanges).
                orphan.OriginalPolicy = originalPolicy;
            }

            if (orphans.Count > 0)
            {
                _logger.LogInformation(
                    "Linked {Count} orphan zeyil(s) to original policy {PolicyNumber}",
                    orphans.Count, originalPolicy.PolicyNumber);
            }

            return orphans.Count;
        }
    }
}
