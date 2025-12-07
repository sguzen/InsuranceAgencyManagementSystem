using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetRevenueByMonth
{
    public class GetRevenueByMonthQuery : IRequest<Result<Dictionary<string, decimal>>>
    {
        public int Months { get; }

        public GetRevenueByMonthQuery(int months = 12)
        {
            Months = months;
        }
    }
}