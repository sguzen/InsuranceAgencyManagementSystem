using AutoMapper;
using FluentValidation;
using IAMS.Shared.QueryParams;
using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using IAMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Commands.UpdateCustomer
{
    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<CustomerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateOrUpdateCustomerDto> _validator;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateCustomerCommandHandler> _logger;

        public UpdateCustomerCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateOrUpdateCustomerDto> validator,
            ICurrentTenantService currentTenantService,
            ICurrentUserService currentUserService,
            ILogger<UpdateCustomerCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
            _currentTenantService = currentTenantService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<CustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                
                // Validate the request DTO
                var validationResult = await _validator.ValidateAsync(request.CustomerDto, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.LogWarning("Customer update validation failed for ID {CustomerId}: {Errors}",
                        request.Id, string.Join(", ", validationErrors));
                    return Result<CustomerDto>.ValidationFailure("Müşteri güncelleme verilerinde hatalar bulundu", validationErrors);
                }

                // Get existing customer and verify tenant ownership
                var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(request.Id);
                if (existingCustomer == null || existingCustomer.IsDeleted)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found for update", request.Id);
                    return Result<CustomerDto>.NotFound("Müşteri bulunamadı");
                }

                // Check for business rule violations
                var businessValidationErrors = await ValidateBusinessRulesAsync(request, existingCustomer);
                if (businessValidationErrors.Any())
                {
                    _logger.LogWarning("Customer update business rule validation failed for ID {CustomerId}: {Errors}",
                        request.Id, string.Join(", ", businessValidationErrors));
                    return Result<CustomerDto>.ValidationFailure(businessValidationErrors.First(), businessValidationErrors);
                }

                // Update the customer
                var originalEmail = existingCustomer.Email;
                var originalIdentificationNo = existingCustomer.IdentificationNumber;
                var originalPhone = existingCustomer.Phone;
                var originalFirstName = existingCustomer.FirstName;
                var originalLastName = existingCustomer.LastName;
                var originalDateOfBirth = existingCustomer.DateOfBirth;

                _mapper.Map(request.CustomerDto, existingCustomer);

                // Preserve system fields
                existingCustomer.ModifiedOn = DateTime.UtcNow;
                existingCustomer.ModifiedBy = _currentUserService.UserName ?? "System";

                _unitOfWork.Customers.Update(existingCustomer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var customerDto = _mapper.Map<CustomerDto>(existingCustomer);

                _logger.LogInformation("Customer {CustomerId} updated successfully. Changes: Email({OriginalEmail}→{NewEmail}), KKTC({OriginalKktc}→{NewKktc}), Phone({OriginalPhone}→{NewPhone})",
                    existingCustomer.Id, originalEmail, existingCustomer.Email, originalIdentificationNo, existingCustomer.IdentificationNumber, originalPhone, existingCustomer.Phone);

                return Result<CustomerDto>.Success(customerDto, "Müşteri başarıyla güncellendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating customer with ID: {CustomerId} for tenant: {TenantId}",
                    request.Id, _currentTenantService.TenantId);

                var errorDetails = new List<string>
                {
                    ex.Message
                };

                // Add inner exception details if available
                if (ex.InnerException != null)
                {
                    errorDetails.Add($"İç hata: {ex.InnerException.Message}");
                }

                return Result<CustomerDto>.InternalError("Müşteri güncellenirken bir hata oluştu", errorDetails);
            }
        }

        private async Task<List<string>> ValidateBusinessRulesAsync(UpdateCustomerCommand request, Customer existingCustomer)
        {
            var errors = new List<string>();

            try
            {
                // Check if email is being changed and if new email already exists
                if (existingCustomer.Email != request.CustomerDto.Email)
                {
                    var duplicateEmail = await _unitOfWork.Customers.GetByEmailAsync(request.CustomerDto.Email);
                    if (duplicateEmail != null && duplicateEmail.Id != request.Id)
                    {
                        errors.Add("Bu e-posta adresi başka bir müşteri tarafından kullanılıyor");
                    }
                }

                // Check if KKTC number is being changed and if new KKTC number already exists
                if (!string.IsNullOrEmpty(request.CustomerDto.IdentificationNumber) &&
                    existingCustomer.IdentificationNumber != request.CustomerDto.IdentificationNumber)
                {
                    var duplicateKktc = await _unitOfWork.Customers.GetByIdentificationNoAsync(request.CustomerDto.IdentificationNumber);
                    if (duplicateKktc != null && duplicateKktc.Id != request.Id)
                    {
                        errors.Add($"Bu KKTC kimlik numarası ({request.CustomerDto.IdentificationNumber}) başka bir müşteri tarafından kullanılmaktadır");
                    }
                }

                // Check if phone number is being changed and if new phone number already exists
                if (existingCustomer.Phone != request.CustomerDto.MobilePhoneNumber)
                {
                    var duplicatePhone = await _unitOfWork.Customers.GetByPhoneAsync(request.CustomerDto.MobilePhoneNumber);
                    if (duplicatePhone != null && duplicatePhone.Id != request.Id)
                    {
                        errors.Add($"Bu telefon numarası ({request.CustomerDto.MobilePhoneNumber}) başka bir müşteri tarafından kullanılmaktadır");
                    }
                }

                // Business rule: Cannot change customer to inactive if they have active policies
                if (request.CustomerDto.Status == Domain.Enums.CustomerStatus.Inactive &&
                    existingCustomer.Status == Domain.Enums.CustomerStatus.Active)
                {
                    var activePolicies = await _unitOfWork.Policies.GetPoliciesByCustomerIdAsync(request.Id);
                    if (activePolicies.Any())
                    {
                        errors.Add("Aktif poliçeleri olan müşteri pasif duruma getirilemez. Önce poliçeleri sonlandırın.");
                    }
                }

                // Business rule: Cannot modify blacklisted customers (except to remove blacklist)
                if (existingCustomer.Status == Domain.Enums.CustomerStatus.Blacklisted &&
                    request.CustomerDto.Status == Domain.Enums.CustomerStatus.Blacklisted)
                {
                    errors.Add("Kara listede olan müşteriler düzenlenemez. Önce kara listeden çıkarın.");
                }

                // Add more business rules as needed...

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during business rule validation for customer {CustomerId}", request.Id);
                errors.Add("İş kuralları doğrulanırken bir hata oluştu");
            }

            return errors;
        }
    }
}
