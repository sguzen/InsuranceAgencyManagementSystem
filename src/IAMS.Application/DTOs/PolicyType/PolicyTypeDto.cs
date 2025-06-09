namespace IAMS.Application.DTOs.PolicyType
{
    public class PolicyTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public int TotalPolicies { get; set; }
    }
}