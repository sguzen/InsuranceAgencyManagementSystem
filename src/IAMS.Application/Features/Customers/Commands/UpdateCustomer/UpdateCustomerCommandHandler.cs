using MediatR;
using AutoMapper;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Services;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Commands.UpdateCustomer
{
    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<CustomerDto>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICustomerValidator _customerValidator;
        private readonly ILogger<UpdateCustomerCommandHandler> _logger;

        public UpdateCustomerCommandHandler(
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICustomerValidator customerValidator,
            ILogger<UpdateCustomerCommandHandler> logger)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _customerValidator = customerValidator;
            _logger = logger;
        }

        public async Task<Result<CustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating customer with ID: {CustomerId}", request.Id);

                // Get existing customer
                var customer = await _customerRepository.GetByIdAsync(request.Id);
                if (customer == null)
                {
                    return Result<CustomerDto>.NotFound("Customer not found");
                }

                // Update customer properties
                customer.UpdatePersonalInfo(
                    request.CustomerDto.FirstName,
                    request.CustomerDto.LastName,
                    request.CustomerDto.TcNo,
                    request.CustomerDto.DateOfBirth,
                    request.CustomerDto.Gender,
                    "System"); // TODO: Get from current user context

                customer.UpdateContactInfo(
                    request.CustomerDto.Email,
                    request.CustomerDto.Phone,
                    request.CustomerDto.Address,
                    "System"); // TODO: Get from current user context

                customer.UpdateStatus(request.CustomerDto.Status, "System");
                customer.UpdateNotes(request.CustomerDto.Notes, "System");

                // Validate updated customer
                var validationErrors = await _customerValidator.ValidateCustomerAsync(customer);
                if (validationErrors.Any())
                {
                    return Result<CustomerDto>.Failure("Validation failed", validationErrors);
                }

                // Save changes
                _customerRepository.Update(customer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map to DTO and return
                var customerDto = _mapper.Map<CustomerDto>(customer);

                _logger.LogInformation("Customer updated successfully: {CustomerId}", customer.Id);

                return Result<CustomerDto>.Success(customerDto, "Customer updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer with ID: {CustomerId}", request.Id);
                return Result<CustomerDto>.InternalError("An error occurred while updating the customer");
            }
        }
    }
}