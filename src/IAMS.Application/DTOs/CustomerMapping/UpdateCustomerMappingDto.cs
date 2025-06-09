namespace IAMS.Application.DTOs.CustomerMapping
{
    public class UpdateCustomerMappingDto
    {
        public string ExternalCustomerId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}