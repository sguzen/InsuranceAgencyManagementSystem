using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.DTOs.Claim
{
    public class PolicyClaimDto
    {
        public int Id { get; set; }
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string ClaimNumber { get; set; } = string.Empty;
        public decimal ClaimAmount { get; set; }
        public DateTime ClaimDate { get; set; }
        public DateTime IncidentDate { get; set; }
        public ClaimStatus Status { get; set; }
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}