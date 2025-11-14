using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<IEnumerable<Invoice>> GetByPolicyIdAsync(int policyId);
        Task<IEnumerable<Invoice>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
        Task<IEnumerable<Invoice>> GetInvoicesByStatusAsync(PaymentStatus status);
        Task<IEnumerable<Invoice>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Invoice?> GetWithItemsAsync(int id);
    }
}
