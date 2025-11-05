using IAMS.Application.DTOs.Currency;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.ConvertMoney
{
    public class ConvertMoneyQuery : IRequest<Result<MoneyDto>>
    {
        public MoneyDto Money { get; set; }
        public int ToCurrencyId { get; set; }
        public DateTime? Date { get; set; }

        public ConvertMoneyQuery(MoneyDto money, int toCurrencyId, DateTime? date = null)
        {
            Money = money;
            ToCurrencyId = toCurrencyId;
            Date = date;
        }
    }
}
