using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Enums
{
    public enum IdentificationType
    {
        KkTcNo = 0,           // Turkish Cypriot ID
        Passport = 1,
        TaxNumber = 2,      // For corporate customers
        TradeLicense = 3    // For business customers
    }
}
