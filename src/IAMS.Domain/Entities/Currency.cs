using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Currency parametric table with exchange rates
    /// </summary>
    public class Currency : BaseEntity
    {
        public string Code { get; set; } = string.Empty; // TRY, USD, EUR, GBP
        public string Symbol { get; set; } = string.Empty; // ₺, $, €, £
        public string Name { get; set; } = string.Empty; // Turkish Lira, US Dollar
        public string NameTr { get; set; } = string.Empty; // Türk Lirası, Amerikan Doları
        public int DecimalPlaces { get; set; } = 2;
        public bool IsActive { get; set; } = true;
        public bool IsBaseCurrency { get; set; } // Main currency for agency (TRY)
        public int DisplayOrder { get; set; }

        public virtual ICollection<PolicyPayment> PolicyPayments { get; set; } = new List<PolicyPayment>();

    }
}
