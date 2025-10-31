using IAMS.Domain.Enums;

namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Vehicle entity for car insurance policies
    /// </summary>
    public class Vehicle : BaseEntity
    {
        // Plate Information
        public string PlateNumber { get; set; } = string.Empty;

        // Vehicle Identification
        public string ChassisNumber { get; set; } = string.Empty; // Şasi Numarası
        public string? EngineNumber { get; set; } // Motor Numarası
        public string? RegistrationNumber { get; set; } // Ruhsat Seri No

        // Vehicle Details
        public int BrandId { get; set; } // Marka (parametric)
        public int ModelId { get; set; } // Model (parametric)
        public int? ModelYear { get; set; } // Model Yılı
        public VehicleType VehicleType { get; set; } // Araç Tipi
        public VehicleFuelType FuelType { get; set; } // Yakıt Tipi
        public VehicleUsageType UsageType { get; set; } // Kullanım Tarzı (Hususi, Ticari, etc.)
        public string? Color { get; set; } // Renk

        // Technical Details
        public int? EngineVolume { get; set; } // Motor Hacmi (cc)
        public int? EnginePower { get; set; } // Motor Gücü (HP)
        public int? SeatCount { get; set; } // Koltuk Sayısı
        public decimal? Weight { get; set; } // Ağırlık (kg)
        public int? LoadCapacity { get; set; } // Yük Kapasitesi (kg)

        // Registration & Inspection
        public DateTime? FirstRegistrationDate { get; set; } // İlk Tescil Tarihi
        public DateTime? TrafficRegistrationDate { get; set; } // Trafik Tescil Tarihi
        public DateTime? LastInspectionDate { get; set; } // Son Muayene Tarihi
        public DateTime? NextInspectionDate { get; set; } // Sonraki Muayene Tarihi

        // Value
        public decimal? CurrentValue { get; set; } // Güncel Değer
        public int CurrencyId { get; set; } = 1; // Default to TRY
        

        // Additional Info
        public string? Notes { get; set; }

        // Owner Information (might be different from policy holder)
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        // Navigation Properties
        public virtual VehicleBrand Brand { get; set; } = null!;
        public virtual VehicleModel Model { get; set; } = null!;
        public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();
        public virtual Currency Currency { get; set; } = null!;

        // Computed Properties
        public int VehicleAge => ModelYear.HasValue ? DateTime.Today.Year - ModelYear.Value : 0;
        public bool IsInspectionDue => NextInspectionDate.HasValue && NextInspectionDate.Value <= DateTime.Today.AddDays(30);
        public string FullName => $"{Brand?.Name} {Model?.Name} - {PlateNumber}";
    }
}