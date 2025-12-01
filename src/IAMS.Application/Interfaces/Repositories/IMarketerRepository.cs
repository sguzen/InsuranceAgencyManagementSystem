using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Marketer entity
    /// </summary>
    public interface IMarketerRepository : IRepository<Marketer>
    {
        /// <summary>
        /// Gets a marketer by their code
        /// </summary>
        Task<Marketer?> GetByCodeAsync(string code);

        /// <summary>
        /// Gets all active marketers
        /// </summary>
        Task<IEnumerable<Marketer>> GetActiveMarketersAsync();

        /// <summary>
        /// Gets marketers with their policy counts
        /// </summary>
        Task<IEnumerable<(Marketer Marketer, int PolicyCount)>> GetMarketersWithPolicyCountAsync();
    }
}
