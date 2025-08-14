using AutoMapper;
using FluentValidation;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Queries.GetCustomer
{
    public class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, Result<CustomerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ILogger<GetCustomerQueryHandler> _logger;

        public GetCustomerQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentTenantService currentTenantService,
            ILogger<GetCustomerQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentTenantService = currentTenantService;
            _logger = logger;
        }

        public async Task<Result<CustomerDto>> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if tenant context is available
                if (!_currentTenantService.HasTenant || _currentTenantService.TenantId == null)
                {
                    _logger.LogError("No tenant context available for customer retrieval");
                    return Result<CustomerDto>.Unauthorized("Kiracı bağlamı bulunamadı. Lütfen tekrar giriş yapın.");
                }

                var tenantId = _currentTenantService.TenantId.Value;

                // Validate request
                if (request.Id <= 0)
                {
                    _logger.LogWarning("Invalid customer ID {CustomerId} provided for tenant {TenantId}", request.Id, tenantId);
                    return Result<CustomerDto>.ValidationFailure("Geçersiz müşteri ID", new List<string> { "Müşteri ID pozitif bir sayı olmalıdır" });
                }

                // Get customer with details
                var customer = await _unitOfWork.Customers.GetByIdWithDetailsAsync(request.Id);

                if (customer == null)
                {
                    _logger.LogInformation("Customer with ID {CustomerId} not found", request.Id);
                    return Result<CustomerDto>.NotFound("Müşteri bulunamadı");
                }

                // Check if customer is soft deleted
                if (customer.IsDeleted)
                {
                    _logger.LogInformation("Attempted to access deleted customer {CustomerId}}", request.Id);
                    return Result<CustomerDto>.NotFound("Müşteri bulunamadı (silinmiş)");
                }

                // Map to DTO
                var customerDto = _mapper.Map<CustomerDto>(customer);

                // Enrich with additional information if needed
                await EnrichCustomerDataAsync(customerDto, customer, tenantId, cancellationToken);

                _logger.LogDebug("Customer {CustomerId} retrieved successfully for tenant {TenantId}", request.Id, tenantId);

                return Result<CustomerDto>.Success(customerDto, "Müşteri başarıyla getirildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while retrieving customer with ID: {CustomerId} for tenant: {TenantId}",
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

                return Result<CustomerDto>.InternalError("Müşteri getirilirken beklenmeyen bir hata oluştu", errorDetails);
            }
        }

        private async Task EnrichCustomerDataAsync(CustomerDto customerDto, Customer customer, int tenantId, CancellationToken cancellationToken)
        {
            try
            {
                // Add policy count and summary information
                if (customer.Policies?.Any() == true)
                {
                    customerDto.ActivePolicyCount = customer.Policies.Count(p => p.Status == Domain.Enums.PolicyStatus.Active && !p.IsDeleted);
                    customerDto.TotalPolicyCount = customer.Policies.Count(p => !p.IsDeleted);
                    customerDto.HasActivePolicies = customerDto.ActivePolicyCount > 0;

                    // Get latest policy information
                    var latestPolicy = customer.Policies
                        .Where(p => !p.IsDeleted)
                        .OrderByDescending(p => p.CreatedOn)
                        .FirstOrDefault();

                    if (latestPolicy != null)
                    {
                        customerDto.LastPolicyDate = latestPolicy.CreatedOn;
                        customerDto.LastPolicyNumber = latestPolicy.PolicyNumber;
                    }
                }
                else
                {
                    customerDto.ActivePolicyCount = 0;
                    customerDto.TotalPolicyCount = 0;
                    customerDto.HasActivePolicies = false;
                }

                // Add customer relationship information
                if (customer.CustomerInsuranceCompanies?.Any() == true)
                {
                    customerDto.InsuranceCompanyCount = customer.CustomerInsuranceCompanies.Count;
                }

                // Calculate customer tenure
                customerDto.CustomerTenureInDays = (DateTime.UtcNow - customer.CreatedOn).Days;

                // Add risk assessment or scoring if available
                customerDto.CustomerScore = await CalculateCustomerScoreAsync(customer, tenantId);

                // Add last activity information
                customerDto.LastActivityDate = await GetLastCustomerActivityAsync(customer.Id, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error enriching customer data for customer {CustomerId}", customer.Id);
                // Don't fail the main operation if enrichment fails
            }
        }

        private async Task<decimal?> CalculateCustomerScoreAsync(Customer customer, int tenantId)
        {
            try
            {
                // Simple scoring algorithm - can be enhanced based on business rules
                decimal score = 0;

                // Base score for being an active customer
                if (customer.Status == Domain.Enums.CustomerStatus.Active)
                    score += 20;

                // Score based on number of policies
                var activePolicyCount = customer.Policies?.Count(p => p.Status == Domain.Enums.PolicyStatus.Active && !p.IsDeleted) ?? 0;
                score += activePolicyCount * 15;

                // Score based on customer tenure
                var tenureYears = (DateTime.UtcNow - customer.CreatedOn).Days / 365.0;
                score += (decimal)(tenureYears * 10);

                // Score based on payment history (if available)
                // var paymentScore = await CalculatePaymentScore(customer.Id);
                // score += paymentScore;

                // Penalty for claims (if excessive)
                // var claimsPenalty = await CalculateClaimsPenalty(customer.Id);
                // score -= claimsPenalty;

                return Math.Max(0, Math.Min(100, score)); // Clamp between 0-100
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating customer score for customer {CustomerId}", customer.Id);
                return null;
            }
        }

        private async Task<DateTime?> GetLastCustomerActivityAsync(int customerId, int tenantId)
        {
            try
            {
                // Get the most recent activity across policies, payments, claims, etc.
               // var lastPolicyActivity = await _unitOfWork.Policies.GetRecentPoliciesAsync(1, tenantId);
                var lastPaymentActivity = await _unitOfWork.PolicyPayments.GetLastPaymentDateAsync(customerId);
                var lastClaimActivity = await _unitOfWork.PolicyClaims.GetLastClaimDateAsync(customerId);

                var activities = new List<DateTime?>
                {
                  //  lastPolicyActivity,
                    lastPaymentActivity,
                    lastClaimActivity
                }.Where(d => d.HasValue).ToList();

                return activities.Any() ? activities.Max() : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting last activity for customer {CustomerId}", customerId);
                return null;
            }
        }
    }
}
