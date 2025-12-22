using IAMS.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace IAMS.Shared.DTOs.Policy
{
    public class UpdatePolicyDto
    {
        [Required(ErrorMessage = "Poliçe numarası zorunludur")]
        [StringLength(50, ErrorMessage = "Poliçe numarası en fazla 50 karakter olabilir")]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Müşteri seçimi zorunludur")]
        public int CustomerId { get; set; }

        // Sigortalı (Insured Customer) - The customer who is insured
        [Required(ErrorMessage = "Sigortalı müşteri seçimi zorunludur")]
        public int InsuredCustomerId { get; set; }

        [Required(ErrorMessage = "Sigorta şirketi seçimi zorunludur")]
        public int InsuranceCompanyId { get; set; }

        [Required(ErrorMessage = "Poliçe tipi seçimi zorunludur")]
        public int PolicyTypeId { get; set; }

        // Vehicle Information (for car insurance policies)
        public int? VehicleId { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi zorunludur")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Prim tutarı zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Prim tutarı 0'dan büyük olmalıdır")]
        public decimal PremiumAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Komisyon tutarı 0'dan küçük olamaz")]
        public decimal CommissionAmount { get; set; }

        [Range(0, 100, ErrorMessage = "Komisyon oranı 0-100 arasında olmalıdır")]
        public decimal CommissionRate { get; set; }

        [Required(ErrorMessage = "Durum seçimi zorunludur")]
        public PolicyStatus Status { get; set; }

        [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Para birimi zorunludur")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Para birimi 3 karakter olmalıdır")]
        public string Currency { get; set; } = "TRY";

        // Optional fields for advanced features
        [StringLength(100, ErrorMessage = "Dış sistem referansı en fazla 100 karakter olabilir")]
        public string? ExternalReference { get; set; }

        public bool IsAutoRenewal { get; set; } = false;

        [Range(1, 365, ErrorMessage = "Hatırlatma günü 1-365 arasında olmalıdır")]
        public int? ReminderDays { get; set; } = 30;

        // Additional premium details
        [Range(0, double.MaxValue, ErrorMessage = "Sigorta tutarı 0'dan küçük olamaz")]
        public decimal InsuredAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Kesinti tutarı 0'dan küçük olamaz")]
        public decimal? DeductibleAmount { get; set; }

        // Payment related
        public PaymentFrequency PaymentFrequency { get; set; } = PaymentFrequency.Annual;

        [Range(1, 12, ErrorMessage = "Ödeme sayısı 1-12 arasında olmalıdır")]
        public int NumberOfPayments { get; set; } = 1;
    }
}
