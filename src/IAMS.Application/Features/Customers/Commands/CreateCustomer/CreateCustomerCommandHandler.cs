using AutoMapper;
using FluentValidation;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Interfaces;
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
        private readonly IValidator<CreateCustomerDto> _validator;
        private readonly ILogger<CreateCustomerCommandHandler> _logger;

        public CreateCustomerCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateCustomerDto> validator,
            ILogger<CreateCustomerCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate the request
                var validationResult = await _validator.ValidateAsync(request.CustomerDto, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Result<CustomerDto>.Failure("Validation failed", errors);
                }

                // Check if TC No already exists
                var existingCustomer = await _unitOfWork.Customers.GetByTcNoAsync(request.CustomerDto.TcNo);
                if (existingCustomer != null)
                {
                    return Result<CustomerDto>.Failure("A customer with this TC number already exists.");
                }

                // Create the customer
                var customer = _mapper.Map<Customer>(request.CustomerDto);
                customer.CreatedOn = DateTime.UtcNow;

                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var customerDto = _mapper.Map<CustomerDto>(customer);
                _logger.LogInformation("Customer created successfully with ID: {CustomerId}", customer.Id);

                return Result<CustomerDto>.Success(customerDto, "Customer created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return Result<CustomerDto>.Failure("An error occurred while creating the customer");
            }
        }
    }
}