using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Entities
{
    public class Occupation : BaseEntity
    {
        public string Code { get; set; } = string.Empty; // Unique occupation code
        public string NameTr { get; set; } = string.Empty; // Turkish name
        public string NameEn { get; set; } = string.Empty; // English name
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        // Navigation properties
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}
