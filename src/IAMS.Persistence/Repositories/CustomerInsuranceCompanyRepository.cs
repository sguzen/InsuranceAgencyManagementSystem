using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class CustomerInsuranceCompanyRepository : Repository<CustomerInsuranceCompany>, ICustomerInsuranceCompanyRepository
    {
        public CustomerInsuranceCompanyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CustomerInsuranceCompany>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(cic => cic.InsuranceCompany)
                .Where(cic => cic.CustomerId == customerId && !cic.IsDeleted)
                .OrderBy(cic => cic.InsuranceCompany.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomerInsuranceCompany>> GetByInsuranceCompanyIdAsync(int companyId)
        {
            return await _dbSet
                .Include(cic => cic.Customer)
                .Where(cic => cic.InsuranceCompanyId == companyId && !cic.IsDeleted)
                .OrderBy(cic => cic.Customer.LastName)
                .ThenBy(cic => cic.Customer.FirstName)
                .ToListAsync();
        }

        public async Task<CustomerInsuranceCompany?> GetMappingAsync(int customerId, int companyId)
        {
            return await _dbSet
                .Include(cic => cic.Customer)
                .Include(cic => cic.InsuranceCompany)
                .Where(cic => !cic.IsDeleted)
                .FirstOrDefaultAsync(cic => cic.CustomerId == customerId &&
                                           cic.InsuranceCompanyId == companyId);
        }

        public async Task<string?> GetExternalCustomerIdAsync(int customerId, int companyId)
        {
            var mapping = await _dbSet
                .Where(cic => cic.CustomerId == customerId &&
                             cic.InsuranceCompanyId == companyId &&
                             cic.IsActive && !cic.IsDeleted)
                .FirstOrDefaultAsync();

            return mapping?.ExternalCustomerId;
        }
    }
}