using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Entities
{
    public class CustomerMapping : BaseEntity
    {
        public int TenantId { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public int InsuranceCompanyId { get; set; }
        public InsuranceCompany InsuranceCompany { get; set; }
        public string ExternalCustomerId { get; set; } // ID in insurance company's system
        public DateTime LastSynced { get; set; }
    }

}
