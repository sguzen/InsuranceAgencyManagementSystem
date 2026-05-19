using IAMS.Application.Features.NexusLeads.Commands.CreateNexusLead;
using IAMS.Shared.DTOs.NexusLead;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace IAMS.Api.Controllers
{
    /// <summary>
    /// Acts as the "Mailbox" for incoming leads from the Nexus server.
    /// Receives extracted data from documents/images processed by the Nexus Bot
    /// and maps them directly to Customer entities in the Agency database.
    /// </summary>
    [ApiController]
    [Route("api/incoming-nexus-lead")]
    public class NexusLeadsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NexusLeadsController> _logger;

        public NexusLeadsController(
            IMediator mediator,
            IConfiguration configuration,
            ILogger<NexusLeadsController> logger)
        {
            _mediator = mediator;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Accepts an ExtractionResult JSON from the Nexus server and maps it
        /// to a new Customer entity in the Agency database.
        /// </summary>
        /// <param name="incomingData">The extracted data payload from the Nexus server.</param>
        /// <param name="signature">HMAC or secret signature to verify the request came from the Nexus server.</param>
        /// <returns>Result of the customer creation process.</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ReceiveNexusLead(
            [FromBody] ExtractionResult incomingData,
            [FromHeader(Name = "X-Nexus-Signature")] string? signature = null)
        {
            // --- 1. Security Check: Verify the request came from the Nexus server ---
            if (!ValidateNexusSignature(signature))
            {
                _logger.LogWarning(
                    "Unauthorized Nexus lead received. Invalid or missing X-Nexus-Signature header. IP: {ClientIp}",
                    HttpContext.Connection.RemoteIpAddress);

                return Unauthorized(new { Message = "Invalid or missing signature." });
            }

            // --- 2. Validate the incoming payload ---
            if (incomingData == null || incomingData.ExtractedData == null)
            {
                _logger.LogWarning("Nexus lead received with empty or invalid payload.");
                return BadRequest(new { Message = "Invalid payload. ExtractionResult and ExtractedData are required." });
            }

            if (!incomingData.Success)
            {
                _logger.LogInformation("Nexus lead reported extraction failure: {ErrorMessage}", incomingData.ErrorMessage);
                return BadRequest(new { Message = "Extraction was not successful.", Error = incomingData.ErrorMessage });
            }

            // --- 3. Dispatch the MediatR command to handle the business logic ---
            var command = new CreateNexusLeadCommand(incomingData);
            var result = await _mediator.Send(command);

            // --- 4. Return appropriate HTTP response ---
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    Message = "Client recorded successfully.",
                    CustomerId = result.Data?.Id,
                    CustomerCode = result.Data?.CustomerCode,
                    FullName = result.Data?.FullName
                });
            }

            // Handle specific failure types
            if (result.StatusCode == 409)
            {
                return Conflict(new { Message = result.Message, Errors = result.Errors });
            }

            if (result.StatusCode == 422)
            {
                return UnprocessableEntity(new { Message = result.Message, Errors = result.Errors });
            }

            return StatusCode(result.StatusCode, new { Message = result.Message, Errors = result.Errors });
        }

        /// <summary>
        /// Validates the X-Nexus-Signature header against the configured secret.
        /// </summary>
        private bool ValidateNexusSignature(string? signatureHeader)
        {
            var expectedSecret = _configuration["NexusSettings:SecretHandshakeKey"];

            // If no secret is configured, allow the request (for local development only!)
            if (string.IsNullOrWhiteSpace(expectedSecret))
            {
                _logger.LogWarning("NexusSettings:SecretHandshakeKey is not configured. Allowing request without validation.");
                return true;
            }

            if (string.IsNullOrWhiteSpace(signatureHeader))
            {
                return false;
            }

            // Simple plaintext comparison (for production, consider HMACSHA256)
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(signatureHeader),
                Encoding.UTF8.GetBytes(expectedSecret));
        }
    }
}
