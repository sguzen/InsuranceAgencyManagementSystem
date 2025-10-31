using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Historical exchange rates for currency conversion
    /// </summary>
    public class CurrencyExchangeRate : BaseEntity
    {
        public int FromCurrencyId { get; set; }
        public int ToCurrencyId { get; set; }
        public decimal Rate { get; set; } // 1 FromCurrency = Rate * ToCurrency
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Source { get; set; } // TCMB, Manual, API

        // Navigation
        public virtual Currency FromCurrency { get; set; } = null!;
        public virtual Currency ToCurrency { get; set; } = null!;

        public bool IsActive => !ExpiryDate.HasValue || ExpiryDate.Value > DateTime.UtcNow;
    }
}
