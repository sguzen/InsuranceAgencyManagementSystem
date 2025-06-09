namespace IAMS.Application.DTOs.CustomerMapping
{
    public class CreateCustomerMappingDto
    {
        public int CustomerId { get; set; }
        public int InsuranceCompanyId { get; set; }
        public string ExternalCustomerId { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
