using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using IAMS.Application.Models;
using IAMS.Shared.DTOs.Customer;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerByPhone
{
    public class GetCustomerByPhoneQuery : IRequest<Result<CustomerDto>>
    {
        public string Phone { get; set; } = string.Empty;
        public int TentandId { get; set; }
    }
}