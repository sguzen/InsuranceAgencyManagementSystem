using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Enums
{
    public enum ClaimStatus
    {
        Submitted = 0,
        UnderReview = 1,
        InvestigationInProgress = 2,
        RequiresAdditionalInfo = 3,
        Approved = 4,
        Rejected = 5,
        Settled = 6,
        Closed = 7
    }
}
