using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Vehicle model parametric table (Model)
    /// </summary>
    public class VehicleModel : BaseEntity
    {
        public int BrandId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        // Navigation
        public virtual VehicleBrand Brand { get; set; } = null!;
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
