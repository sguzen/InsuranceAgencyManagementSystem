namespace IAMS.Application.DTOs.InsuranceCompany
{
    public class InsuranceCompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public int TotalPolicies { get; set; }
        public int ActivePolicies { get; set; }
    }
}