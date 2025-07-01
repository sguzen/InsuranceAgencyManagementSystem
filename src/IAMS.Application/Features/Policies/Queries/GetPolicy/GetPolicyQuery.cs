using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.Features.Policies.Queries.GetPolicy
{
    public class GetPolicyQuery : IRequest<Result<PolicyDto>>
    {
        public int Id { get; set; }

        public GetPolicyQuery(int id)
        {
            Id = id;
        }
    }
}
