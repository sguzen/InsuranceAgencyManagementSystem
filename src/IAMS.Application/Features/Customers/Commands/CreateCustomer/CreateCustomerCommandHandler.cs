using AutoMapper;
using IAMS.Shared.QueryParams;
using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using IAMS.Application.Services;
using IAMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using IAMS.Shared.DTOs.Customer;
using IAMS.Application.Models;
using IAMS.Shared.Interfaces;

namespace IAMS.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICustomerCodeGenerator _customerCodeGenerator;
        private readonly ILogger<CreateCustomerCommandHandler> _logger;

        public CreateCustomerCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentTenantService currentTenantService,
            ICurrentUserService currentUserService,
            ICustomerCodeGenerator customerCodeGenerator,
            ILogger<CreateCustomerCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentTenantService = currentTenantService;
            _currentUserService = currentUserService;
            _customerCodeGenerator = customerCodeGenerator;
            _logger = logger;
        }

        public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if customer with identification number already exists for this tenant (if provided)
                if (!string.IsNullOrEmpty(request.CustomerDto.IdentificationNumber))
                {
                    var existingCustomerById = await _unitOfWork.Customers.GetByIdentificationNoAsync(request.CustomerDto.IdentificationNumber);

                    if (existingCustomerById != null)
                    {
                        _logger.LogWarning("Customer creation failed: Identification number already exists");
                        return Result<CustomerDto>.ValidationFailure("Bu T.C. Kimlik Numarası ile kayıtlı bir müşteri zaten mevcut", new List<string>());
                    }
                }

                // E-posta is hidden in the UI; generate a unique placeholder when blank
                // (same scheme as the policy-import paths — Email has a unique index).
                if (string.IsNullOrWhiteSpace(request.CustomerDto.Email))
                {
                    request.CustomerDto.Email = $"noemail_{Guid.NewGuid():N}@temp.com";
                }
                else
                {
                    // Check if customer with email already exists for this tenant
                    var existingCustomerByEmail = await _unitOfWork.Customers.GetByEmailAsync(request.CustomerDto.Email);
                    if (existingCustomerByEmail != null)
                    {
                        _logger.LogWarning("Customer creation failed: Email already exists");
                        return Result<CustomerDto>.ValidationFailure("Bu e-posta adresi ile kayıtlı bir müşteri zaten mevcut", new List<string>());
                    }
                }

                // Check if customer with phone number already exists for this tenant
                var existingCustomerByPhone = await _unitOfWork.Customers.GetByPhoneAsync(request.CustomerDto.MobilePhoneNumber);
                if (existingCustomerByPhone != null)
                {
                    _logger.LogWarning("Customer creation failed: Phone number already exists");
                    return Result<CustomerDto>.ValidationFailure($"Bu telefon numarası ({request.CustomerDto.MobilePhoneNumber}) zaten kullanılmaktadır", new List<string>());
                }

                // Map and create customer
                var customer = _mapper.Map<Customer>(request.CustomerDto);
                customer.CreatedBy = _currentUserService.UserName ?? "System";
                customer.CreatedOn = DateTime.UtcNow;

                // Generate customer code using the enhanced generator
                customer.CustomerCode = await _customerCodeGenerator.GenerateAsync();

                // Verify the generated code is unique (extra safety check)
                if (!await _customerCodeGenerator.IsCodeUniqueAsync(customer.CustomerCode))
                {
                    _logger.LogError("Generated customer code {Code} is not unique",
                        customer.CustomerCode);
                    customer.CustomerCode = _customerCodeGenerator.GenerateFallbackCode();
                }

                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var customerDto = _mapper.Map<CustomerDto>(customer);
                _logger.LogInformation("Customer created successfully with ID: {CustomerId} and Code: {CustomerCode}",
                    customer.Id, customer.CustomerCode);

                return Result<CustomerDto>.Success(customerDto, "Müşteri başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while creating customer with email: {Email} for tenant: {TenantId}",
                    request.CustomerDto.Email, _currentTenantService.TenantId);

                // Check for duplicate key exceptions
                var exceptionMessage = ex.InnerException?.Message ?? ex.Message;

                if (exceptionMessage.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
                    exceptionMessage.Contains("IX_Customers_IdentificationNumber", StringComparison.OrdinalIgnoreCase))
                {
                    return Result<CustomerDto>.ValidationFailure("Bu T.C. Kimlik Numarası ile kayıtlı bir müşteri zaten mevcut", new List<string>());
                }

                if (exceptionMessage.Contains("IX_Customers_Email", StringComparison.OrdinalIgnoreCase))
                {
                    return Result<CustomerDto>.ValidationFailure("Bu e-posta adresi ile kayıtlı bir müşteri zaten mevcut", new List<string>());
                }

                if (exceptionMessage.Contains("IX_Customers_MobilePhoneNumber", StringComparison.OrdinalIgnoreCase))
                {
                    return Result<CustomerDto>.ValidationFailure("Bu telefon numarası zaten kullanılmaktadır", new List<string>());
                }

                var errorDetails = new List<string>
                {
                    ex.Message
                };

                // Add inner exception details if available
                if (ex.InnerException != null)
                {
                    errorDetails.Add($"İç hata: {ex.InnerException.Message}");
                }

                return Result<CustomerDto>.InternalError("Müşteri oluşturulurken bir hata oluştu", errorDetails);
            }
        }
    }
}