using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Shared.DTOs.Currency
{
    public class CurrencyConversionDto
    {
        public MoneyDto OriginalAmount { get; set; } = null!;
        public MoneyDto ConvertedAmount { get; set; } = null!;
        public decimal ExchangeRate { get; set; }
        public DateTime ConversionDate { get; set; }
        public int TargetCurrencyId { get; internal set; }
        public int OriginalCurrencyId { get; internal set; }
        public string CurrencyCode { get; internal set; }
    }
}
