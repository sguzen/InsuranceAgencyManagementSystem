using AutoMapper;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateCustomerCommandHandler> _logger;

        public CreateCustomerCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateCustomerCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if customer with email already exists
                var existingCustomer = await _unitOfWork.Customers.GetByEmailAsync(request.CustomerDto.Email);
                if (existingCustomer != null)
                {
                    return Result<CustomerDto>.Failure($"Customer with email {request.CustomerDto.Email} already exists");
                }

                var customer = _mapper.Map<Customer>(request.CustomerDto);
                customer.CreatedBy = "System"; // TODO: Get from current user context

                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var customerDto = _mapper.Map<CustomerDto>(customer);
                _logger.LogInformation("Customer created successfully with ID: {CustomerId}", customer.Id);

                return Result<CustomerDto>.Success(customerDto, "Customer created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating customer");
                return Result<CustomerDto>.Failure("An error occurred while creating the customer");
            }
        }
    }
}