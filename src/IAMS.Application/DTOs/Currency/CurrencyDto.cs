using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.DTOs.Currency
{
    public class CurrencyDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NameTr { get; set; } = string.Empty;
        public int DecimalPlaces { get; set; }
        public bool IsActive { get; set; }
        public bool IsBaseCurrency { get; set; }
    }

}
