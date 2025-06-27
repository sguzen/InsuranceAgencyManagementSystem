using IAMS.Application.Models;
using IAMS.Domain.Enums;

namespace IAMS.Application.DTOs.Policy
{
    public class PolicyQueryParams : PagedQueryParams
    {
        public PolicyStatus? Status { get; set; }
        public int? CustomerId { get; set; }
        public int? InsuranceCompanyId { get; set; }
        public int? PolicyTypeId { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public bool? IsExpiring { get; set; } // Next 30 days
        public bool? IsExpired { get; set; }
        public decimal? MinPremium { get; set; }
        public decimal? MaxPremium { get; set; }
    }
}