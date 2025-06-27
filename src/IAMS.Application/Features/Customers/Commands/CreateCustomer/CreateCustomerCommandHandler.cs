using AutoMapper;
using FluentValidation;
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
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICustomerCodeGenerator _customerCodeGenerator;
        private readonly ICustomerValidator _customerValidator;
        private readonly ILogger<CreateCustomerCommandHandler> _logger;

        public CreateCustomerCommandHandler(
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICustomerCodeGenerator customerCodeGenerator,
            ICustomerValidator customerValidator,
            ILogger<CreateCustomerCommandHandler> logger)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _customerCodeGenerator = customerCodeGenerator;
            _customerValidator = customerValidator;
            _logger = logger;
        }

        public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating customer: {FirstName} {LastName}",
                    request.CustomerDto.FirstName, request.CustomerDto.LastName);

                // Generate customer code
                var customerCode = await _customerCodeGenerator.GenerateCustomerCodeAsync(
                    1, // TODO: Get from tenant context
                    request.CustomerDto.FirstName,
                    request.CustomerDto.LastName);

                // Create customer entity
                var customer = Customer.Create(
                    customerCode,
                    request.CustomerDto.FirstName,
                    request.CustomerDto.LastName,
                    request.CustomerDto.TcNo,
                    request.CustomerDto.DateOfBirth,
                    request.CustomerDto.Gender,
                    request.CustomerDto.Email,
                    request.CustomerDto.Phone,
                    request.CustomerDto.Address,
                    "System", // TODO: Get from current user context
                    1, // TODO: Get from tenant context
                    request.CustomerDto.Notes);

                // Validate customer
                var validationErrors = await _customerValidator.ValidateCustomerAsync(customer);
                if (validationErrors.Any())
                {
                    return Result<CustomerDto>.Failure("Validation failed", validationErrors);
                }

                // Save to database
                await _customerRepository.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map to DTO and return
                var customerDto = _mapper.Map<CustomerDto>(customer);

                _logger.LogInformation("Customer created successfully with ID: {CustomerId}", customer.Id);

                return Result<CustomerDto>.Success(customerDto, "Customer created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer: {FirstName} {LastName}",
                    request.CustomerDto.FirstName, request.CustomerDto.LastName);

                return Result<CustomerDto>.InternalError("An error occurred while creating the customer");
            }
        }
    }
}