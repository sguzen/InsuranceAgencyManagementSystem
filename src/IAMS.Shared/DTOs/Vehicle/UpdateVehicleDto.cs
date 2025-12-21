using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Shared.DTOs.Vehicle
{
    public class UpdateVehicleDto
    {
        public string? Color { get; set; }
        public decimal? CurrentValue { get; set; }
        public DateTime? LastInspectionDate { get; set; }
        public DateTime? NextInspectionDate { get; set; }
        public bool HasAlarm { get; set; }
        public bool HasImmobilizer { get; set; }
        public string? Notes { get; set; }
    }
}
