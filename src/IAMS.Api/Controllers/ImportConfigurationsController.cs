using IAMS.Application.Features.ImportConfigurations.Commands.CreateImportConfiguration;
using IAMS.Application.Features.ImportConfigurations.Commands.DeleteImportConfiguration;
using IAMS.Application.Features.ImportConfigurations.Commands.UpdateImportConfiguration;
using IAMS.Application.Features.ImportConfigurations.Queries.GetImportConfiguration;
using IAMS.Application.Features.ImportConfigurations.Queries.GetImportConfigurations;
using IAMS.Application.Features.ImportConfigurations.Queries.GetImportConfigurationsByInsuranceCompany;
using IAMS.Application.Models;
using IAMS.Shared.DTOs.ImportConfiguration;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiKeyOrJwt")]
    public class ImportConfigurationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ImportConfigurationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all active import configurations
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<Result<List<ImportConfigurationDto>>>> GetImportConfigurations()
        {
            var query = new GetImportConfigurationsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get import configuration by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Result<ImportConfigurationDto>>> GetImportConfiguration(int id)
        {
            var query = new GetImportConfigurationQuery(id);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Get import configurations by insurance company ID
        /// </summary>
        [HttpGet("by-company/{insuranceCompanyId}")]
        public async Task<ActionResult<Result<List<ImportConfigurationDto>>>> GetByInsuranceCompany(
            int insuranceCompanyId)
        {
            var query = new GetImportConfigurationsByInsuranceCompanyQuery(insuranceCompanyId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Create a new import configuration
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Result<ImportConfigurationDto>>> CreateImportConfiguration(
            [FromBody] CreateImportConfigurationDto configurationDto)
        {
            var command = new CreateImportConfigurationCommand(configurationDto);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetImportConfiguration),
                new { id = result.Data!.Id }, result);
        }

        /// <summary>
        /// Update an existing import configuration
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Result<ImportConfigurationDto>>> UpdateImportConfiguration(
            int id,
            [FromBody] UpdateImportConfigurationDto configurationDto)
        {
            var command = new UpdateImportConfigurationCommand(id, configurationDto);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Delete an import configuration (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Result>> DeleteImportConfiguration(int id)
        {
            var command = new DeleteImportConfigurationCommand(id);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
