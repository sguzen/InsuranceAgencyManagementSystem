using IAMS.Application.DTOs.Parametric;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Parametric.Commands.SyncCountryData
{
    /// <summary>
    /// Handler for synchronizing countries from external DAS API
    /// </summary>
    public class SyncCountryDataCommandHandler : IRequestHandler<SyncCountryDataCommand, Result<CountryDataSyncResult>>
    {
        private readonly ICountryDataService _countryDataService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SyncCountryDataCommandHandler> _logger;

        public SyncCountryDataCommandHandler(
            ICountryDataService countryDataService,
            IUnitOfWork unitOfWork,
            ILogger<SyncCountryDataCommandHandler> logger)
        {
            _countryDataService = countryDataService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<CountryDataSyncResult>> Handle(
            SyncCountryDataCommand request,
            CancellationToken cancellationToken)
        {
            var result = new CountryDataSyncResult { Success = true };

            try
            {
                _logger.LogInformation("Starting country data synchronization from external API");

                // Fetch data from external API
                var externalData = await _countryDataService.FetchCountryDataAsync();

                if (externalData == null || !externalData.Any())
                {
                    _logger.LogWarning("No country data received from external API");
                    result.Warnings.Add("No data received from external API");
                    return Result<CountryDataSyncResult>.Success(result, "No data to synchronize");
                }

                _logger.LogInformation("Fetched {Count} country records from external API", externalData.Count);

                // Get existing countries
                var existingCountries = await _unitOfWork.Countries.GetAllAsync();

                // Process each country from external data
                var displayOrder = 0;
                foreach (var countryData in externalData.OrderBy(c => c.DisplayOrder ?? c.Id))
                {
                    try
                    {
                        var code = countryData.Code?.Trim();
                        var nameTr = countryData.Name?.Trim();

                        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(nameTr))
                        {
                            result.Warnings.Add($"Skipping country with invalid code or name: {countryData.Code}");
                            continue;
                        }

                        var existingCountry = existingCountries.FirstOrDefault(c =>
                            c.Code == code ||
                            c.NameTr.Equals(nameTr, StringComparison.OrdinalIgnoreCase));

                        if (existingCountry == null)
                        {
                            // Create new country
                            var newCountry = new Country
                            {
                                Code = code,
                                NameTr = nameTr,
                                NameEn = countryData.NameEn?.Trim() ?? nameTr,
                                PhoneCode = countryData.PhoneCode?.Trim(),
                                IsActive = countryData.IsActive ?? true,
                                DisplayOrder = displayOrder++
                            };

                            await _unitOfWork.Countries.AddAsync(newCountry);
                            result.CountriesCreated++;
                            _logger.LogDebug("Created new country: {CountryName} (Code: {CountryCode})", nameTr, code);
                        }
                        else if (request.UpdateExisting)
                        {
                            // Update existing country
                            var countryUpdated = false;

                            if (existingCountry.Code != code)
                            {
                                existingCountry.Code = code;
                                countryUpdated = true;
                            }

                            if (existingCountry.NameTr != nameTr)
                            {
                                existingCountry.NameTr = nameTr;
                                countryUpdated = true;
                            }

                            var nameEn = countryData.NameEn?.Trim() ?? nameTr;
                            if (existingCountry.NameEn != nameEn)
                            {
                                existingCountry.NameEn = nameEn;
                                countryUpdated = true;
                            }

                            var phoneCode = countryData.PhoneCode?.Trim();
                            if (existingCountry.PhoneCode != phoneCode)
                            {
                                existingCountry.PhoneCode = phoneCode;
                                countryUpdated = true;
                            }

                            var isActive = countryData.IsActive ?? true;
                            if (existingCountry.IsActive != isActive)
                            {
                                existingCountry.IsActive = isActive;
                                countryUpdated = true;
                            }

                            if (countryUpdated)
                            {
                                _unitOfWork.Countries.Update(existingCountry);
                                result.CountriesUpdated++;
                                _logger.LogDebug("Updated country: {CountryName} (Code: {CountryCode})", nameTr, code);
                            }
                        }

                        // Save changes after each country to avoid losing all progress on error
                        await _unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error processing country {CountryName} (Code: {CountryCode})",
                            countryData.Name,
                            countryData.Code);
                        result.Warnings.Add($"Failed to process country: {countryData.Name}");
                    }
                }

                // Handle deactivation of missing items if requested
                if (request.DeactivateMissing)
                {
                    var externalCodes = externalData
                        .Select(x => x.Code?.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Distinct()
                        .ToHashSet();

                    foreach (var country in existingCountries.Where(c => c.IsActive && !externalCodes.Contains(c.Code)))
                    {
                        country.IsActive = false;
                        _unitOfWork.Countries.Update(country);
                        result.Warnings.Add($"Deactivated country not found in external data: {country.NameTr}");
                    }

                    await _unitOfWork.SaveChangesAsync();
                }

                _logger.LogInformation(
                    "Country data synchronization completed. Countries: {CountriesCreated} created, {CountriesUpdated} updated",
                    result.CountriesCreated,
                    result.CountriesUpdated);

                var message = $"Synchronization completed: {result.CountriesCreated} countries created, {result.CountriesUpdated} countries updated";
                return Result<CountryDataSyncResult>.Success(result, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during country data synchronization");
                result.Success = false;
                result.ErrorMessage = $"Synchronization failed: {ex.Message}";
                return Result<CountryDataSyncResult>.Failure(
                    message: result.ErrorMessage,
                    data: result,
                    errors: null,
                    statusCode: 400);
            }
        }
    }
}
