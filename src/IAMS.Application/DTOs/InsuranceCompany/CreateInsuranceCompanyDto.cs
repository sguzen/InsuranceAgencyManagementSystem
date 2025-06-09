namespace IAMS.Application.DTOs.InsuranceCompany
{
    public class CreateInsuranceCompanyDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public bool IsActive { get; set; } = true;
    }
}