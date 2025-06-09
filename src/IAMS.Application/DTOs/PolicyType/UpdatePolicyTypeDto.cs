namespace IAMS.Application.DTOs.PolicyType
{
    public class UpdatePolicyTypeDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}