using IAMS.Shared.Models;

namespace IAMS.Shared.QueryParams
{
    public class InsuranceCompanyQueryParams : PagedQueryParams
    {
        public bool? IsActive { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public bool? HasActivePolicies { get; set; }
    }
}
