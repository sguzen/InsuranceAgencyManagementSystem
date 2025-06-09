using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Enums
{
    public enum UserStatus
    {
        Active = 0,
        Inactive = 1,
        Suspended = 2,
        PendingActivation = 3,
        Locked = 4
    }
}
