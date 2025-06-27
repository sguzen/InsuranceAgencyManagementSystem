using IAMS.Application.Models;

namespace IAMS.Application.DTOs.InsuranceCompany
{
    public class InsuranceCompanyQueryParams : PagedQueryParams
    {
        public bool? IsActive { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public bool? HasActivePolicies { get; set; }
    }
}