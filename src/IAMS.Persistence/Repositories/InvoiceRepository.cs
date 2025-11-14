using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            return await _dbSet
                .Include(i => i.Policy)
                .Include(i => i.Customer)
                .Include(i => i.Currency)
                .Include(i => i.InvoiceItems)
                .Where(i => i.InvoiceNumber == invoiceNumber && !i.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByPolicyIdAsync(int policyId)
        {
            return await _dbSet
                .Include(i => i.Policy)
                .Include(i => i.Customer)
                .Include(i => i.Currency)
                .Include(i => i.InvoiceItems)
                .Where(i => i.PolicyId == policyId && !i.IsDeleted)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(i => i.Policy)
                .Include(i => i.Customer)
                .Include(i => i.Currency)
                .Include(i => i.InvoiceItems)
                .Where(i => i.CustomerId == customerId && !i.IsDeleted)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
        {
            var today = DateTime.Today;
            return await _dbSet
                .Include(i => i.Policy)
                .Include(i => i.Customer)
                .Include(i => i.Currency)
                .Include(i => i.InvoiceItems)
                .Where(i => i.PaymentStatus == PaymentStatus.Pending &&
                           i.DueDate < today &&
                           !i.IsDeleted)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByStatusAsync(PaymentStatus status)
        {
            return await _dbSet
                .Include(i => i.Policy)
                .Include(i => i.Customer)
                .Include(i => i.Currency)
                .Include(i => i.InvoiceItems)
                .Where(i => i.PaymentStatus == status && !i.IsDeleted)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(i => i.Policy)
                .Include(i => i.Customer)
                .Include(i => i.Currency)
                .Include(i => i.InvoiceItems)
                .Where(i => i.InvoiceDate >= startDate &&
                           i.InvoiceDate <= endDate &&
                           !i.IsDeleted)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<Invoice?> GetWithItemsAsync(int id)
        {
            return await _dbSet
                .Include(i => i.Policy)
                .Include(i => i.Customer)
                .Include(i => i.Currency)
                .Include(i => i.InvoiceItems)
                .Where(i => i.Id == id && !i.IsDeleted)
                .FirstOrDefaultAsync();
        }
    }
}
