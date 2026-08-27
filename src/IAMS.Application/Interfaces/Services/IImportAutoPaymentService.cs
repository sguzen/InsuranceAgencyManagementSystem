using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces.Services
{
    /// <summary>
    /// Single home for the "traffic-family policies are fully paid at creation" import rule,
    /// shared by all policy-import paths (Excel, preview/mapping, MySQL sync).
    /// </summary>
    public interface IImportAutoPaymentService
    {
        /// <summary>
        /// Note text stored on auto-created payments so they remain recognizable.
        /// </summary>
        string AutoPaymentNote { get; }

        /// <summary>
        /// Note text for the negative payment created for a negative-premium zeyil
        /// (iade), keeping the traffic-policy chain at zero balance.
        /// </summary>
        string AutoRefundNote { get; }

        /// <summary>
        /// True when the policy type is in the traffic family covered by the law:
        /// its code, name, or category contains Trafik/Traffic/Trafic or any Kasko
        /// variant (Kasko, Yarım Kasko, Tam Kasko, ...), in any casing or spelling.
        /// </summary>
        bool RequiresFullPaymentByLaw(PolicyType policyType);

        /// <summary>
        /// Combines <see cref="RequiresFullPaymentByLaw"/> with the agency's
        /// AutoPayMandatoryPolicies setting (default on). The setting is read once per
        /// import run (per request scope), so a batch is never split between modes.
        /// </summary>
        Task<bool> ShouldAutoPayAsync(PolicyType policyType, CancellationToken cancellationToken = default);
    }
}
