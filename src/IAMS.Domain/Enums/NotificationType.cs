using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Enums
{
    public enum NotificationType
    {
        PolicyExpiry = 0,
        PaymentDue = 1,
        PaymentReceived = 2,
        ClaimUpdate = 3,
        PolicyRenewal = 4,
        SystemAlert = 5,
        UserAction = 6,
        Integration = 7
    }
}
