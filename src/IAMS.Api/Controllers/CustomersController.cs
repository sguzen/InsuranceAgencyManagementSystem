using IAMS.Application.DTOs.Customer;
using IAMS.Application.DTOs.Identity;
using IAMS.Application.Services.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetById(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerDto customerDto)
        {
            var createdCustomer = await _customerService.CreateAsync(customerDto);
            return CreatedAtAction(nameof(GetById), new { id = createdCustomer.Id }, createdCustomer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto customerDto)
        {
            try
            {
                await _customerService.UpdateAsync(id, customerDto);
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _customerService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
