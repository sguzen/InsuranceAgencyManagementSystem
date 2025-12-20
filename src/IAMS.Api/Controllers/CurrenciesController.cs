using IAMS.Application.DTOs.Currency;
using IAMS.Application.Features.Currencies.Commands.UpdateExchangeRatesFromTCMB;
using IAMS.Application.Features.Currencies.Queries.GetCurrencies;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CurrenciesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CurrenciesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all active currencies
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<Result<List<CurrencyDto>>>> GetCurrencies()
        {
            var query = new GetCurrenciesQuery();
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Update exchange rates from Turkish Central Bank (TCMB)
        /// </summary>
        /// <remarks>
        /// This endpoint fetches current exchange rates from TCMB (Türkiye Cumhuriyet Merkez Bankası)
        /// and populates the CurrencyExchangeRate table with the latest rates.
        ///
        /// The TCMB publishes daily exchange rates for major currencies (USD, EUR, GBP, etc.) to TRY.
        /// This endpoint will:
        /// - Fetch the latest rates from https://www.tcmb.gov.tr/kurlar/today.xml
        /// - Parse the XML response
        /// - Create/update exchange rate records in the database
        /// - Set the effective date to today and expiry date to tomorrow
        ///
        /// Note: This should be called regularly (e.g., daily) to keep exchange rates up to date.
        /// You can also set up a background job to automate this process.
        ///
        /// </remarks>
        [HttpPost("update-from-tcmb")]
        public async Task<ActionResult<Result>> UpdateExchangeRatesFromTCMB()
        {
            var command = new UpdateExchangeRatesFromTCMBCommand();
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
