using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Commands.PopulatePolicyExchangeRates
{
    /// <summary>
    /// Command to populate exchange rates for existing policies that don't have them set
    /// </summary>
    public class PopulatePolicyExchangeRatesCommand : IRequest<Result<PopulatePolicyExchangeRatesResult>>
    {
        /// <summary>
        /// If true, only processes policies that don't have exchange rates set.
        /// If false, recalculates exchange rates for all policies.
        /// </summary>
        public bool OnlyMissing { get; set; } = true;

        /// <summary>
        /// If true, uses the policy creation date to get historical exchange rates.
        /// If false, uses current exchange rates for all policies.
        /// </summary>
        public bool UseHistoricalRates { get; set; } = true;
    }

    public class PopulatePolicyExchangeRatesResult
    {
        public int TotalPolicies { get; set; }
        public int ProcessedPolicies { get; set; }
        public int SkippedPolicies { get; set; }
        public int FailedPolicies { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
