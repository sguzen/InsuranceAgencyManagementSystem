using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Vehicle brand parametric table (Marka)
    /// </summary>
    public class VehicleBrand : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        // Navigation
        public virtual ICollection<VehicleModel> Models { get; set; } = new List<VehicleModel>();
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
