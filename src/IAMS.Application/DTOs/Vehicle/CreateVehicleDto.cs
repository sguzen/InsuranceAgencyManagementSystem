using IAMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.DTOs.Vehicle
{
    public class CreateVehicleDto
    {
        public string PlateNumber { get; set; } = string.Empty;
        public string? PlateCity { get; set; }
        public string ChassisNumber { get; set; } = string.Empty;
        public string? EngineNumber { get; set; }
        public string? RegistrationNumber { get; set; }
        public int BrandId { get; set; }
        public int ModelId { get; set; }
        public int? ModelYear { get; set; }
        public VehicleType VehicleType { get; set; }
        public VehicleFuelType FuelType { get; set; }
        public VehicleUsageType UsageType { get; set; }
        public string? Color { get; set; }
        public int? EngineVolume { get; set; }
        public int? EnginePower { get; set; }
        public int? SeatCount { get; set; }
        public decimal? Weight { get; set; }
        public int? LoadCapacity { get; set; }
        public DateTime? FirstRegistrationDate { get; set; }
        public DateTime? TrafficRegistrationDate { get; set; }
        public DateTime? LastInspectionDate { get; set; }
        public DateTime? NextInspectionDate { get; set; }
        public decimal? CurrentValue { get; set; }
        public string Currency { get; set; } = "TRY";
        public bool IsNewVehicle { get; set; }
        public bool HasAlarm { get; set; }
        public bool HasImmobilizer { get; set; }
        public string? Notes { get; set; }
        public int CustomerId { get; set; }
    }
}
