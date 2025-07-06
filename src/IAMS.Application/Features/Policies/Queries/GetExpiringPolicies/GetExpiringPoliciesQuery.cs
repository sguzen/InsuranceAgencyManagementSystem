using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.Features.Policies.Queries.GetExpiringPolicies
{
    public class GetExpiringPoliciesQuery : IRequest<Result<List<PolicyDto>>>
    {
        public int DaysAhead { get; }

        public GetExpiringPoliciesQuery(int daysAhead = 30)
        {
            DaysAhead = daysAhead;
        }
    }
}
