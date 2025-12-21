using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Shared.DTOs.Vehicle
{
    public class VehicleBrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
