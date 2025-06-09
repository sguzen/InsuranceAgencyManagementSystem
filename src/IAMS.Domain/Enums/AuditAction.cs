using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Enums
{
    public enum AuditAction
    {
        Create = 0,
        Update = 1,
        Delete = 2,
        View = 3,
        Export = 4,
        Import = 5,
        Login = 6,
        Logout = 7,
        PasswordChange = 8,
        RoleChange = 9
    }
}
