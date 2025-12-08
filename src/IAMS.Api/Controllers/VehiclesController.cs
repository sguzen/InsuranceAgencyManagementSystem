using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Features.Vehicles.Commands.SyncVehicleData;
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
 //   [Authorize]
    public class VehiclesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IVehicleDataService _vehicleDataService;

        public VehiclesController(IMediator mediator, IVehicleDataService vehicleDataService)
        {
            _mediator = mediator;
            _vehicleDataService = vehicleDataService;
        }

        /// <summary>
        /// Synchronize vehicle brands and models from external API
        /// </summary>
        /// <param name="updateExisting">Whether to update existing records (default: true)</param>
        /// <param name="deactivateMissing">Whether to deactivate brands/models not found in external data (default: false)</param>
        /// <returns>Result with sync statistics</returns>
        [HttpPost("sync")]
        public async Task<ActionResult<Result<VehicleDataSyncResult>>> SyncVehicleData(
            [FromQuery] bool updateExisting = true,
            [FromQuery] bool deactivateMissing = false)
        {
            var command = new SyncVehicleDataCommand
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
        /// Test connection to external vehicle data API
        /// </summary>
        /// <returns>Connection test result</returns>
        [HttpGet("test-connection")]
        public async Task<ActionResult<Result>> TestConnection()
        {
            var isConnected = await _vehicleDataService.TestConnectionAsync();

            if (isConnected)
            {
                return Ok(Result.Success("Connection to vehicle data API successful"));
            }

            // Explicitly specify null for the second parameter to resolve ambiguity
            return BadRequest(Result.Failure("Failed to connect to vehicle data API", (List<string>?)null));
        }

        /// <summary>
        /// Fetch vehicle data from external API (for preview/testing)
        /// </summary>
        /// <returns>List of vehicle data from external API</returns>
        [HttpGet("fetch-external-data")]
        public async Task<ActionResult<Result<List<ExternalVehicleDataDto>>>> FetchExternalData()
        {
            try
            {
                var data = await _vehicleDataService.FetchVehicleDataAsync();
                return Ok(Result<List<ExternalVehicleDataDto>>.Success(
                    data,
                    $"Successfully fetched {data.Count} vehicle records from external API"));
            }
            catch (Exception ex)
            {
                // Specify null for errors and omit statusCode to resolve ambiguity
                return BadRequest(Result<List<ExternalVehicleDataDto>>.Failure(
                    $"Failed to fetch vehicle data: {ex.Message}", (List<string>?)null));
            }
        }
    }
}
