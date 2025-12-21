using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Shared.DTOs.Vehicle
{
    public class VehicleModelDto
    {
        public int Id { get; set; }
        public int BrandId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public bool IsActive { get; set; }
    }
}
