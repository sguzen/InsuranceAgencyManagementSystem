using IAMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.DTOs.Vehicle
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string ChassisNumber { get; set; } = string.Empty;
        public string? EngineNumber { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public int? ModelYear { get; set; }
        public VehicleType VehicleType { get; set; }
        public VehicleFuelType FuelType { get; set; }
        public VehicleUsageType UsageType { get; set; }
        public string? Color { get; set; }
        public decimal? CurrentValue { get; set; }
        public string Currency { get; set; } = "TRY";
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int VehicleAge { get; set; }
        public bool IsInspectionDue { get; set; }
        public string FullName => $"{BrandName} {ModelName} - {PlateNumber}";
    }
}
