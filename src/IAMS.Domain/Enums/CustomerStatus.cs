using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Enums
{
    public enum CustomerStatus
    {
        Active = 0,
        Inactive = 1,
        Suspended = 2,
        Blacklisted = 3,
        Prospect = 4,
        Former = 5
    }

}
