using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.DTOs.Claim
{
    public class CreatePolicyClaimDto
    {
        public int PolicyId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public decimal ClaimAmount { get; set; }
        public DateTime ClaimDate { get; set; }
        public DateTime IncidentDate { get; set; }
        public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;
        public string? Description { get; set; }
        public string? Notes { get; set; }
    }
}