using IAMS.Application.Interfaces.Services;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Shared.DTOs.Policy;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Interfaces.Services;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IAMS.Application.Features.Policies.Queries.PreviewMySqlImport
{
    public class PreviewMySqlImportQueryHandler : IRequestHandler<PreviewMySqlImportQuery, Result<MySqlImportPreviewResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMySqlPolicyImportService _mySqlImportService;
        private readonly IAgencyCredentialService _credentialService;
        private readonly ILogger<PreviewMySqlImportQueryHandler> _logger;

        public PreviewMySqlImportQueryHandler(
            IUnitOfWork unitOfWork,
            IMySqlPolicyImportService mySqlImportService,
            IAgencyCredentialService credentialService,
            ILogger<PreviewMySqlImportQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mySqlImportService = mySqlImportService;
            _credentialService = credentialService;
            _logger = logger;
        }

        public async Task<Result<MySqlImportPreviewResult>> Handle(
            PreviewMySqlImportQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Find the tenant's InsuranceCompany that corresponds to the master DB ID
                var tenantInsuranceCompany = await _unitOfWork.InsuranceCompanies
                    .GetByMasterIdAsync(request.MasterInsuranceCompanyId);

                if (tenantInsuranceCompany == null)
                {
                    return Result<MySqlImportPreviewResult>.Failure(
                        $"Bu sigorta sirketi tenant veritabaninda bulunamadi (MasterID={request.MasterInsuranceCompanyId}). " +
                        "Sigorta sirketi senkronizasyonunu kontrol edin.", (List<string>?)null);
                }

                // Build ImportConfiguration from master DB credentials
                var credentials = await _credentialService.GetCredentialDetailsAsync(request.AgencyId, request.MasterInsuranceCompanyId);
                if (credentials == null)
                {
                    return Result<MySqlImportPreviewResult>.Failure(
                        "Bu sigorta sirketi icin veritabani baglanti bilgileri bulunamadi.", (List<string>?)null);
                }

                var importConfig = BuildImportConfiguration(credentials, tenantInsuranceCompany.Id);

                // Default date range: last year to today
                var effectiveStartDate = request.StartDate ?? DateTime.Today.AddYears(-1);
                var effectiveEndDate = request.EndDate ?? DateTime.Today;

                _logger.LogInformation(
                    "Fetching MySQL policies for preview: Company={CompanyName}, DateRange={Start:yyyy-MM-dd} to {End:yyyy-MM-dd}",
                    tenantInsuranceCompany.Name, effectiveStartDate, effectiveEndDate);

                // Fetch policies from MySQL (read-only, no save)
                var policies = await _mySqlImportService.FetchPoliciesAsync(
                    importConfig, effectiveStartDate, effectiveEndDate, cancellationToken);

                _logger.LogInformation("Fetched {Count} policies from MySQL for preview", policies.Count);

                // Convert to preview DTOs with default Cari Kart mapping
                var previewDtos = policies.Select((dto, index) => new PolicyImportPreviewDto
                {
                    RowNumber = dto.RowNumber > 0 ? dto.RowNumber : index + 1,
                    PolicyNumber = dto.PolicyNumber,
                    InnerCode = dto.InnerCode,
                    StateType = dto.StateType,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    PolicyTypeCode = dto.PolicyTypeCode,
                    PolicyTypeName = dto.PolicyTypeName,
                    PremiumAmount = dto.PremiumAmount,
                    CommissionRate = dto.CommissionRate,
                    CommissionAmount = dto.CommissionAmount,
                    CurrencyCode = dto.CurrencyCode,
                    PlateNumber = dto.PlateNumber,
                    VehicleBrand = dto.VehicleBrand,
                    VehicleModel = dto.VehicleModel,
                    VehicleYear = dto.VehicleYear,
                    InsuredCustomerName = dto.CustomerName,
                    InsuredCustomerIdentifier = dto.CustomerIdentifier,
                    PolicyOwnerSameAsInsured = true, // Default: policy owner = sigortali
                    OriginalImportData = dto
                }).ToList();

                var result = new MySqlImportPreviewResult
                {
                    TenantInsuranceCompanyId = tenantInsuranceCompany.Id,
                    InsuranceCompanyName = tenantInsuranceCompany.Name,
                    Policies = previewDtos
                };

                return Result<MySqlImportPreviewResult>.Success(
                    result,
                    $"{previewDtos.Count} police basariyla okundu. Lutfen musteri eslestirmesi yapin.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching MySQL policies for preview");
                return Result<MySqlImportPreviewResult>.Failure(
                    $"MySQL veritabanindan policeler alinamadi: {ex.Message}", (List<string>?)null);
            }
        }

        private static ImportConfiguration BuildImportConfiguration(AgencyCredentialDetails credentials, int tenantInsuranceCompanyId)
        {
            if (string.IsNullOrWhiteSpace(credentials.AgencyCode))
            {
                throw new InvalidOperationException(
                    "Bu sigorta sirketi icin acenta kodu tanimli degil. " +
                    "Yonetim panelinde acenta-sigorta sirketi baglantisina acenta kodunu (sigortacinin 'ackod' degeri) girin.");
            }

            var additionalSettings = JsonSerializer.Serialize(new
            {
                Port = credentials.Port,
                UseSsl = false,
                ConnectionTimeoutSeconds = 30,
                CommandTimeoutSeconds = 120
            });

            return new ImportConfiguration
            {
                Name = $"Preview-{tenantInsuranceCompanyId}",
                InsuranceCompanyId = tenantInsuranceCompanyId,
                SourceType = ImportSourceType.DatabaseImport,
                IsActive = true,
                ApiBaseUrl = credentials.Host,
                ApiKey = credentials.AgencyCode ?? "",
                ApiUsername = credentials.Username,
                ApiPassword = credentials.Password,
                PoliciesEndpoint = credentials.DatabaseName,
                AdditionalSettings = additionalSettings
            };
        }
    }
}
