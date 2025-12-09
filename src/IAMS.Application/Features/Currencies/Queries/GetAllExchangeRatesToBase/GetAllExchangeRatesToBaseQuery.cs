using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.GetAllExchangeRatesToBase
{
    public class GetAllExchangeRatesToBaseQuery : IRequest<Result<Dictionary<string, decimal>>>
    {
        public DateTime? Date { get; set; }

        public GetAllExchangeRatesToBaseQuery(DateTime? date = null)
        {
            Date = date;
        }
    }
}
