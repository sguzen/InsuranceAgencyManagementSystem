using AutoMapper;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Application.Services;
using IAMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ICustomerCodeGenerator _customerCodeGenerator;
        private readonly ILogger<CreateCustomerCommandHandler> _logger;

        public CreateCustomerCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentTenantService currentTenantService,
            ICustomerCodeGenerator customerCodeGenerator,
            ILogger<CreateCustomerCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentTenantService = currentTenantService;
            _customerCodeGenerator = customerCodeGenerator;
            _logger = logger;
        }

        public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if tenant context is available
                if (!_currentTenantService.HasTenant || _currentTenantService.TenantId == null)
                {
                    _logger.LogError("No tenant context available for customer creation");
                    return Result<CustomerDto>.Unauthorized("Kiracı bağlamı bulunamadı. Lütfen tekrar giriş yapın.");
                }

                var tenantId = _currentTenantService.TenantId.Value;
                var validationErrors = new List<string>();

                // Check if customer with email already exists for this tenant
                var existingCustomerByEmail = await _unitOfWork.Customers.GetByEmailAsync(request.CustomerDto.Email, tenantId);
                if (existingCustomerByEmail != null)
                {
                    validationErrors.Add($"Bu e-posta adresi ({request.CustomerDto.Email}) zaten kullanılmaktadır");
                }

                // Check if customer with KKTC number already exists for this tenant (if provided)
                if (!string.IsNullOrEmpty(request.CustomerDto.KktcNo))
                {
                    var existingCustomerByKktc = await _unitOfWork.Customers.GetByKktcNoAsync(request.CustomerDto.KktcNo, tenantId);
                    
                    if (existingCustomerByKktc != null)
                    {
                        validationErrors.Add($"Bu KKTC kimlik numarası ({request.CustomerDto.KktcNo}) zaten kullanılmaktadır");
                    }
                }

                // Check if customer with phone number already exists for this tenant
                var existingCustomerByPhone = await _unitOfWork.Customers.GetByPhoneAsync(request.CustomerDto.Phone, tenantId);
                if (existingCustomerByPhone != null)
                {
                    validationErrors.Add($"Bu telefon numarası ({request.CustomerDto.Phone}) zaten kullanılmaktadır");
                }

                // If there are validation errors, return them
                if (validationErrors.Any())
                {
                    _logger.LogWarning("Customer creation failed for tenant {TenantId} due to validation errors: {Errors}",
                        tenantId, string.Join(", ", validationErrors));
                    return Result<CustomerDto>.ValidationFailure("Müşteri oluşturulamadı: Girilen bilgiler zaten kullanılmaktadır", validationErrors);
                }

                // Map and create customer
                var customer = _mapper.Map<Customer>(request.CustomerDto);
                customer.CreatedBy = "System"; // TODO: Get from current user context
                customer.CreatedOn = DateTime.UtcNow;

                // Generate customer code using the enhanced generator
                customer.CustomerCode = await _customerCodeGenerator.GenerateAsync(tenantId);

                // Verify the generated code is unique (extra safety check)
                if (!await _customerCodeGenerator.IsCodeUniqueAsync(customer.CustomerCode, tenantId))
                {
                    _logger.LogError("Generated customer code {Code} is not unique for tenant {TenantId}",
                        customer.CustomerCode, tenantId);
                    customer.CustomerCode = _customerCodeGenerator.GenerateFallbackCode();
                }

                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var customerDto = _mapper.Map<CustomerDto>(customer);
                _logger.LogInformation("Customer created successfully for tenant {TenantId} with ID: {CustomerId} and Code: {CustomerCode}",
                    tenantId, customer.Id, customer.CustomerCode);

                return Result<CustomerDto>.Success(customerDto, "Müşteri başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while creating customer with email: {Email} for tenant: {TenantId}",
                    request.CustomerDto.Email, _currentTenantService.TenantId);

                var errorDetails = new List<string>
                {
                    ex.Message
                };

                // Add inner exception details if available
                if (ex.InnerException != null)
                {
                    errorDetails.Add($"İç hata: {ex.InnerException.Message}");
                }

                return Result<CustomerDto>.InternalError("Müşteri oluşturulurken beklenmeyen bir hata oluştu", errorDetails);
            }
        }
    }
}