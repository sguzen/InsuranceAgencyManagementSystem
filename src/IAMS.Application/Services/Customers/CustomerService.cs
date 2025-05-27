using AutoMapper;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Interfaces;
using IAMS.Domain.Entities;

namespace IAMS.Application.Services.Customers
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<CustomerDto>> GetAllAsync()
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            return _mapper.Map<List<CustomerDto>>(customers);
        }

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerDto customerDto)
        {
            // Check if TC No already exists
            var existingCustomer = await _unitOfWork.Customers.GetByTcNoAsync(customerDto.TcNo);
            if (existingCustomer != null)
            {
                throw new InvalidOperationException("A customer with this TC number already exists.");
            }

            var customer = _mapper.Map<Customer>(customerDto);
            customer.CreatedOn = DateTime.UtcNow;

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CustomerDto>(customer);
        }

        public async Task UpdateAsync(int id, UpdateCustomerDto customerDto)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
            {
                throw new InvalidOperationException("Customer not found.");
            }

            _mapper.Map(customerDto, customer);
            customer.ModifiedOn = DateTime.UtcNow;

            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
            {
                throw new InvalidOperationException("Customer not found.");
            }

            // Check if customer has active policies
            var policies = await _unitOfWork.Policies.GetPoliciesByCustomerIdAsync(id);
            if (policies.Any(p => p.Status == PolicyStatus.Active))
            {
                throw new InvalidOperationException("Cannot delete customer with active policies.");
            }

            _unitOfWork.Customers.Remove(customer);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}