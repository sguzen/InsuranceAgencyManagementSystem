using MediatR;
using IAMS.Application.Models;

namespace IAMS.Application.Features.Dashboard.Queries.GetDashboard
{
    public class GetDashboardQuery : IRequest<Result<DashboardDto>>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}