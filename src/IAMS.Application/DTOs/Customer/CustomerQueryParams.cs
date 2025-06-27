using IAMS.Application.Models;
using IAMS.Domain.Enums;

namespace IAMS.Application.DTOs.Customer
{
    public class CustomerQueryParams : PagedQueryParams
    {
        public CustomerStatus? Status { get; set; }
        public Gender? Gender { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public bool? HasActivePolicies { get; set; }
    }
}