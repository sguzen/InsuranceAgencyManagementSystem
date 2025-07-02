using AutoMapper;
using FluentValidation;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Commands.UpdateCustomer
{
    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<CustomerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateOrUpdateCustomerDto> _validator;
        private readonly ILogger<UpdateCustomerCommandHandler> _logger;

        public UpdateCustomerCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateOrUpdateCustomerDto> validator,
            ILogger<UpdateCustomerCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<CustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
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

                // Get existing customer
                var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(request.Id);
                if (existingCustomer == null)
                {
                    return Result<CustomerDto>.NotFound("Customer not found");
                }

                // Check if KKTC No is being changed and if it already exists
                if (existingCustomer.KktcNo != request.CustomerDto.KktcNumber && !string.IsNullOrEmpty(request.CustomerDto.KktcNumber))
                {
                    var duplicateCustomer = await _unitOfWork.Customers.GetByKktcNoAsync(request.CustomerDto.KktcNumber, request.Id);
                    if (duplicateCustomer != null)
                    {
                        return Result<CustomerDto>.Failure("A customer with this KKTC number already exists.");
                    }
                }

                // Update the customer
                _mapper.Map(request.CustomerDto, existingCustomer);
                existingCustomer.ModifiedOn = DateTime.UtcNow;

                _unitOfWork.Customers.Update(existingCustomer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var customerDto = _mapper.Map<CustomerDto>(existingCustomer);
                _logger.LogInformation("Customer updated successfully with ID: {CustomerId}", existingCustomer.Id);

                return Result<CustomerDto>.Success(customerDto, "Customer updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer with ID: {CustomerId}", request.Id);
                return Result<CustomerDto>.Failure("An error occurred while updating the customer");
            }
        }
    }
}