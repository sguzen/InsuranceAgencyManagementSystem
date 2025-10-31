using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.DTOs.Currency
{
    public class CurrencyConversionDto
    {
        public MoneyDto OriginalAmount { get; set; } = null!;
        public MoneyDto ConvertedAmount { get; set; } = null!;
        public decimal ExchangeRate { get; set; }
        public DateTime ConversionDate { get; set; }
    }
}
