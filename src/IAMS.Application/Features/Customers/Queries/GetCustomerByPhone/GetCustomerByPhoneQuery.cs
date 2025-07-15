using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerByPhone
{
    public class GetCustomerByPhoneQuery : IRequest<Result<CustomerDto>>
    {
        public string Phone { get; set; } = string.Empty;
        public int TentandId { get; set; }
    }
}