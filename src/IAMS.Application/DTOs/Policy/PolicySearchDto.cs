using IAMS.Domain.Enums;

namespace IAMS.Application.DTOs.Policy
{
    public class PolicySearchDto
    {
        public string? SearchTerm { get; set; }
        public PolicyStatus? Status { get; set; }
        public int? CustomerId { get; set; }
        public int? InsuranceCompanyId { get; set; }
        public int? PolicyTypeId { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public decimal? MinPremiumAmount { get; set; }
        public decimal? MaxPremiumAmount { get; set; }
        public bool? IsExpiring { get; set; }
        public int? ExpiringInDays { get; set; }
        public bool? IsAutoRenewal { get; set; }
        public PaymentFrequency? PaymentFrequency { get; set; }

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;

        // Sorting
        public string? SortBy { get; set; } = "EndDate";
        public string SortDirection { get; set; } = "asc";
    }
}