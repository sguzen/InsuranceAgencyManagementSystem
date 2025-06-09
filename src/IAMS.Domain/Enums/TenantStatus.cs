using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Enums
{
    public enum TenantStatus
    {
        Active = 0,
        Inactive = 1,
        Suspended = 2,
        Trial = 3,
        Expired = 4,
        PendingSetup = 5
    }

}
