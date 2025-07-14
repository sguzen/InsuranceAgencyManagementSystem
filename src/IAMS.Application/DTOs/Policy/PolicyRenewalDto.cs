using IAMS.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace IAMS.Application.DTOs.Policy
{
    public class PolicyRenewalDto
    {
        [Required(ErrorMessage = "Yeni başlangıç tarihi zorunludur")]
        public DateTime NewStartDate { get; set; }

        [Required(ErrorMessage = "Yeni bitiş tarihi zorunludur")]
        public DateTime NewEndDate { get; set; }

        [Required(ErrorMessage = "Yeni prim tutarı zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Prim tutarı 0'dan büyük olmalıdır")]
        public decimal NewPremiumAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Komisyon tutarı 0'dan küçük olamaz")]
        public decimal NewCommissionAmount { get; set; }

        [Range(0, 100, ErrorMessage = "Komisyon oranı 0-100 arasında olmalıdır")]
        public decimal NewCommissionRate { get; set; }

        [StringLength(1000, ErrorMessage = "Yenileme notları en fazla 1000 karakter olabilir")]
        public string? RenewalNotes { get; set; }

        // Keep some fields from original policy or allow changes
        public bool KeepOriginalTerms { get; set; } = true;

        public bool UpdateInsuredAmount { get; set; } = false;

        [Range(0, double.MaxValue, ErrorMessage = "Sigorta tutarı 0'dan küçük olamaz")]
        public decimal? NewInsuredAmount { get; set; }

        public PaymentFrequency PaymentFrequency { get; set; } = PaymentFrequency.Annual;
    }
}