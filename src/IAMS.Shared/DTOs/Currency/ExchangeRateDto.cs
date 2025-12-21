using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Shared.DTOs.Currency
{
    public class ExchangeRateDto
    {
        public int Id { get; set; }
        public int FromCurrencyId { get; set; }
        public string FromCurrencyCode { get; set; } = string.Empty;
        public string FromCurrencySymbol { get; set; } = string.Empty;
        public int ToCurrencyId { get; set; }
        public string ToCurrencyCode { get; set; } = string.Empty;
        public string ToCurrencySymbol { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Source { get; set; }
        public bool IsActive { get; set; }
    }

}
