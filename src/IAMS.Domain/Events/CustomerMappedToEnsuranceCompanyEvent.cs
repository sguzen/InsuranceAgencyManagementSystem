using IAMS.Domain.Entities;

namespace IAMS.Domain.Events
{
    public class CustomerMappedToInsuranceCompanyEvent : DomainEvent
    {
        public CustomerInsuranceCompany Mapping { get; }
        public string MappedBy { get; }

        public CustomerMappedToInsuranceCompanyEvent(CustomerInsuranceCompany mapping, string mappedBy)
            : base(mapping.TenantId)
        {
            Mapping = mapping;
            MappedBy = mappedBy;
        }
    }
}