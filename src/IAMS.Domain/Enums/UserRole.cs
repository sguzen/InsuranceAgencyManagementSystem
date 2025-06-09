using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Enums
{
    public enum UserRole
    {
        SuperAdmin = 0,     // System-wide admin
        TenantAdmin = 1,    // Agency admin
        Manager = 2,        // Agency manager
        Agent = 3,          // Insurance agent
        Clerk = 4,          // Data entry clerk
        Viewer = 5          // Read-only access
    }
}
