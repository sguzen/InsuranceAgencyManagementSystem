using IAMS.Application.DTOs.PolicyType;
using IAMS.Application.Models;
using IAMS.Application.Services.PolicyTypes;
using IAMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiKeyOrJwt")]
    public class PolicyTypesController : ControllerBase
    {
        private readonly IPolicyTypeService _policyTypeService;

        public PolicyTypesController(IPolicyTypeService policyTypeService)
        {
            _policyTypeService = policyTypeService;
        }

        /// <summary>
        /// Get all policy types
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<Result<List<PolicyTypeDto>>>> GetAll()
        {
            try
            {
                var policyTypes = await _policyTypeService.GetAllAsync();
                return Ok(Result<List<PolicyTypeDto>>.Success(policyTypes));
            }
            catch (Exception ex)
            {
                return BadRequest(Result<List<PolicyTypeDto>>.Failure($"Error retrieving policy types: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get active policy types only
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<Result<List<PolicyTypeDto>>>> GetActiveTypes()
        {
            try
            {
                var policyTypes = await _policyTypeService.GetActiveTypesAsync();
                return Ok(Result<List<PolicyTypeDto>>.Success(policyTypes));
            }
            catch (Exception ex)
            {
                return BadRequest(Result<List<PolicyTypeDto>>.Failure($"Error retrieving active policy types: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get policy type by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Result<PolicyTypeDto>>> GetById(int id)
        {
            try
            {
                var policyType = await _policyTypeService.GetByIdAsync(id);
                if (policyType == null)
                    return NotFound(Result<PolicyTypeDto>.NotFound("Policy type not found"));

                return Ok(Result<PolicyTypeDto>.Success(policyType));
            }
            catch (Exception ex)
            {
                return BadRequest(Result<PolicyTypeDto>.Failure($"Error retrieving policy type: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create a new policy type
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Result<PolicyTypeDto>>> Create([FromBody] CreatePolicyTypeDto policyTypeDto)
        {
            try
            {
                var policyType = await _policyTypeService.CreateAsync(policyTypeDto);
                return CreatedAtAction(nameof(GetById), new { id = policyType.Id }, Result<PolicyTypeDto>.Success(policyType));
            }
            catch (Exception ex)
            {
                return BadRequest(Result<PolicyTypeDto>.Failure($"Error creating policy type: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update an existing policy type
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Result>> Update(int id, [FromBody] UpdatePolicyTypeDto policyTypeDto)
        {
            try
            {
                await _policyTypeService.UpdateAsync(id, policyTypeDto);
                return Ok(Result.Success("Policy type updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(Result.Failure($"Error updating policy type: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a policy type
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Result>> Delete(int id)
        {
            try
            {
                await _policyTypeService.DeleteAsync(id);
                return Ok(Result.Success("Policy type deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(Result.Failure($"Error deleting policy type: {ex.Message}"));
            }
        }
    }
}
