using System.ComponentModel.DataAnnotations;

namespace IAMS.Shared.DTOs.Commission
{
    public class CreateOrUpdateCommissionRateDto
    {
        [Required(ErrorMessage = "Sigorta şirketi gereklidir")]
        public int InsuranceCompanyId { get; set; }

        [Required(ErrorMessage = "Poliçe tipi gereklidir")]
        public int PolicyTypeId { get; set; }

        [Required(ErrorMessage = "Komisyon oranı gereklidir")]
        [Range(0, 100, ErrorMessage = "Komisyon oranı 0 ile 100 arasında olmalıdır")]
        public decimal Rate { get; set; }

        [Required(ErrorMessage = "Para birimi gereklidir")]
        public int CurrencyId { get; set; }

        [Required(ErrorMessage = "Geçerlilik başlangıç tarihi gereklidir")]
        public DateTime EffectiveDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Notes { get; set; }
    }
}
