using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Shared.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Payments
{
    /// <summary>
    /// Service for allocating customer payments to policies using FIFO strategy
    /// </summary>
    public class PaymentAllocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentAllocationService> _logger;

        public PaymentAllocationService(
            IUnitOfWork unitOfWork,
            ILogger<PaymentAllocationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Auto-allocate a customer payment to policies using FIFO strategy (oldest policies first)
        /// </summary>
        public async Task<List<PaymentAllocation>> AllocatePaymentAsync(int customerPaymentId, string allocatedBy)
        {
            var payment = await _unitOfWork.CustomerPayments.GetByIdWithRelationsAsync(customerPaymentId);
            if (payment == null)
                throw new InvalidOperationException($"Payment {customerPaymentId} not found");

            if (payment.UnallocatedAmount <= 0)
            {
                _logger.LogInformation("Payment {PaymentId} is already fully allocated", customerPaymentId);
                return new List<PaymentAllocation>();
            }

            // Get all outstanding policies for this customer in the same currency
            // Order by start date (FIFO - oldest first)
            var policies = await _unitOfWork.Policies.GetPoliciesByCustomerIdAsync(payment.CustomerId);
            var outstandingPolicies = policies
                .Where(p => p.CurrencyId == payment.CurrencyId && p.Status == PolicyStatus.Active)
                .OrderBy(p => p.StartDate)  // FIFO: Oldest first
                .ThenBy(p => p.Id)
                .ToList();

            var allocations = new List<PaymentAllocation>();
            var remainingAmount = payment.UnallocatedAmount;

            foreach (var policy in outstandingPolicies)
            {
                if (remainingAmount <= 0)
                    break;

                // Calculate how much this policy still owes
                var policyAllocated = await GetPolicyAllocatedAmountAsync(policy.Id);
                var policyOutstanding = policy.PremiumAmount - policyAllocated;

                if (policyOutstanding <= 0)
                    continue; // Policy already fully paid

                // Allocate as much as possible to this policy
                var allocationAmount = Math.Min(remainingAmount, policyOutstanding);

                var allocation = new PaymentAllocation
                {
                    CustomerPaymentId = customerPaymentId,
                    PolicyId = policy.Id,
                    AllocatedAmount = allocationAmount,
                    AllocationDate = DateTime.UtcNow,
                    AllocationType = AllocationType.Automatic,
                    CreatedBy = allocatedBy,
                    CreatedOn = DateTime.UtcNow
                };

                allocations.Add(allocation);
                payment.PaymentAllocations.Add(allocation); // Add to navigation property
                payment.AddAllocation(allocationAmount);
                remainingAmount -= allocationAmount;

                _logger.LogInformation(
                    "Allocated {Amount} from payment {PaymentId} to policy {PolicyId}. Remaining: {Remaining}",
                    allocationAmount, customerPaymentId, policy.Id, remainingAmount);
            }

            // Save all allocations by adding them to the payment's collection
            if (allocations.Any())
            {
                // Note: The allocations will be saved through the CustomerPayment's navigation property
                // when SaveChanges is called, as they're already added to payment.PaymentAllocations
                // via the relationship tracking

                _unitOfWork.CustomerPayments.Update(payment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Created {Count} allocations for payment {PaymentId}. Total allocated: {TotalAllocated}, Unallocated: {Unallocated}",
                    allocations.Count, customerPaymentId, payment.AllocatedAmount, payment.UnallocatedAmount);
            }

            return allocations;
        }

        /// <summary>
        /// Calculate total amount allocated to a specific policy from customer payments
        /// </summary>
        private async Task<decimal> GetPolicyAllocatedAmountAsync(int policyId)
        {
            // Query through customer payments to get all allocations for this policy
            var customerPayments = await _unitOfWork.CustomerPayments.AsQueryable()
                .SelectMany(cp => cp.PaymentAllocations)
                .Where(pa => pa.PolicyId == policyId && !pa.IsDeleted)
                .ToListAsync();

            return customerPayments.Sum(pa => pa.AllocatedAmount);
        }
    }
}
