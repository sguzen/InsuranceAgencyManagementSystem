using AutoMapper;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Queries.GetCustomer
{
    public class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, Result<CustomerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCustomerQueryHandler> _logger;

        public GetCustomerQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCustomerQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CustomerDto>> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id);
                if (customer == null)
                {
                    return Result<CustomerDto>.NotFound("Customer not found");
                }

                var customerDto = _mapper.Map<CustomerDto>(customer);

                // Add calculated properties
                var customerPolicies = await _unitOfWork.Policies.GetPoliciesByCustomerIdAsync(request.Id);
                customerDto.PolicyCount = customerPolicies.Count();
                customerDto.TotalPremium = await _unitOfWork.Policies.GetTotalPremiumByCustomerAsync(request.Id);
                customerDto.LastPolicyDate = customerPolicies.OrderByDescending(p => p.CreatedOn).FirstOrDefault()?.CreatedOn;

                return Result<CustomerDto>.Success(customerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer with ID: {CustomerId}", request.Id);
                return Result<CustomerDto>.Failure("An error occurred while retrieving the customer");
            }
        }
    }
}