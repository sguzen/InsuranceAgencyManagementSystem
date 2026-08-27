using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces.Services
{
    /// <summary>
    /// Zeyils (endorsements) may be imported before their original policy exists
    /// (#528). When the original arrives later, this service attaches the orphans
    /// so the chain displays and sums as one policy again.
    /// </summary>
    public interface IEndorsementLinkService
    {
        /// <summary>
        /// Links previously imported orphan endorsements (same insurance company and
        /// policy number, no original set) to this original (InnerCode=000) policy.
        /// The caller's SaveChanges persists the links. Returns the number linked.
        /// </summary>
        Task<int> LinkOrphanEndorsementsAsync(Policy originalPolicy, CancellationToken cancellationToken = default);
    }
}
