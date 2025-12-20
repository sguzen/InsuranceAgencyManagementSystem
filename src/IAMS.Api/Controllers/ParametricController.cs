using IAMS.Application.DTOs.Parametric;
using IAMS.Application.Features.Parametric.Commands.SyncCountryData;
using IAMS.Application.Interfaces;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiKeyOrJwt")]
    public class ParametricController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICountryDataService _countryDataService;
        private readonly ILogger<ParametricController> _logger;

        public ParametricController(
            IMediator mediator,
            ICountryDataService countryDataService,
            ILogger<ParametricController> logger)
        {
            _mediator = mediator;
            _countryDataService = countryDataService;
            _logger = logger;
        }

        /// <summary>
        /// Health check endpoint to verify controller and service registration
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            _logger.LogInformation("Parametric controller health check called");
            return Ok(new {
                status = "OK",
                controller = "ParametricController",
                countryServiceRegistered = _countryDataService != null,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Synchronize countries from external DAS API
        /// </summary>
        /// <param name="updateExisting">Whether to update existing records (default: true)</param>
        /// <param name="deactivateMissing">Whether to deactivate countries not found in external data (default: false)</param>
        /// <returns>Result with sync statistics</returns>
        [HttpPost("countries/sync")]
        public async Task<ActionResult<Result<CountryDataSyncResult>>> SyncCountryData(
            [FromQuery] bool updateExisting = true,
            [FromQuery] bool deactivateMissing = false)
        {
            var command = new SyncCountryDataCommand
            {
                UpdateExisting = updateExisting,
                DeactivateMissing = deactivateMissing
            };

            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Test connection to external country data API
        /// </summary>
        /// <returns>Connection test result</returns>
        [HttpGet("countries/test-connection")]
        public async Task<ActionResult<Result>> TestCountryConnection()
        {
            var isConnected = await _countryDataService.TestConnectionAsync();

            if (isConnected)
            {
                return Ok(Result.Success("Connection to country data API successful"));
            }

            return BadRequest(Result.Failure("Failed to connect to country data API", (List<string>?)null));
        }

        /// <summary>
        /// Fetch country data from external API (for preview/testing)
        /// </summary>
        /// <returns>List of country data from external API</returns>
        [HttpGet("countries/fetch-external-data")]
        public async Task<ActionResult<Result<List<ExternalCountryDataDto>>>> FetchExternalCountryData()
        {
            try
            {
                var data = await _countryDataService.FetchCountryDataAsync();
                return Ok(Result<List<ExternalCountryDataDto>>.Success(
                    data,
                    $"Successfully fetched {data.Count} country records from external API"));
            }
            catch (Exception ex)
            {
                return BadRequest(Result<List<ExternalCountryDataDto>>.Failure(
                    $"Failed to fetch country data: {ex.Message}", (List<string>?)null));
            }
        }
    }
}
