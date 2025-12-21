using IAMS.Shared.DTOs.Customer;
using IAMS.Domain.Enums;
using IAMS.Shared.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerStatement
{
    public class GetCustomerStatementQueryHandler : IRequestHandler<GetCustomerStatementQuery, Result<CustomerMultiCurrencyStatementDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ILogger<GetCustomerStatementQueryHandler> _logger;

        public GetCustomerStatementQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentTenantService currentTenantService,
            ILogger<GetCustomerStatementQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentTenantService = currentTenantService;
            _logger = logger;
        }

        public async Task<Result<CustomerMultiCurrencyStatementDto>> Handle(
            GetCustomerStatementQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Get customer details
                var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
                if (customer == null)
                {
                    return Result<CustomerMultiCurrencyStatementDto>.Failure("Müşteri bulunamadı", (List<string>?)null);
                }

                // Get all policies for the customer
                var policies = await _unitOfWork.Policies.GetPoliciesByCustomerIdAsync(request.CustomerId);
                var policyIds = policies.Select(p => p.Id).ToList();

                // Get all payments for these policies
                var paymentsQuery = _unitOfWork.PolicyPayments
                    .AsQueryable()
                    .Include(p => p.Currency)
                    .Include(p => p.Policy)
                    .Where(p => policyIds.Contains(p.PolicyId));

                var allPayments = await paymentsQuery.ToListAsync(cancellationToken);

                // Get all currencies used
                var currencyIds = policies.Select(p => p.CurrencyId)
                    .Union(allPayments.Select(p => p.CurrencyId))
                    .Distinct()
                    .ToList();

                var currencies = await _unitOfWork.Currencies
                    .AsQueryable()
                    .Where(c => currencyIds.Contains(c.Id))
                    .ToListAsync(cancellationToken);

                // Filter by specific currency if requested
                if (!string.IsNullOrWhiteSpace(request.CurrencyCode))
                {
                    var requestedCurrency = currencies.FirstOrDefault(c =>
                        c.Code.Equals(request.CurrencyCode, StringComparison.OrdinalIgnoreCase));

                    if (requestedCurrency == null)
                    {
                        return Result<CustomerMultiCurrencyStatementDto>.Failure($"Para birimi bulunamadı: {request.CurrencyCode}", (List<string>?)null);
                    }

                    currencies = new List<IAMS.Domain.Entities.Currency> { requestedCurrency };
                }

                // Build customer address
                var customerAddress = BuildCustomerAddress(customer);

                // Create the multi-currency statement
                var multiCurrencyStatement = new CustomerMultiCurrencyStatementDto
                {
                    CustomerId = customer.Id,
                    CustomerCode = customer.CustomerCode,
                    CustomerName = $"{customer.FirstName} {customer.LastName}".Trim(),
                    CustomerAddress = customerAddress,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    GeneratedAt = DateTime.UtcNow
                };

                // Build statement for each currency
                foreach (var currency in currencies)
                {
                    var statement = BuildCurrencyStatement(
                        customer,
                        currency,
                        policies.Where(p => p.CurrencyId == currency.Id).ToList(),
                        allPayments.Where(p => p.CurrencyId == currency.Id).ToList(),
                        request.StartDate,
                        request.EndDate,
                        customerAddress);

                    // Only add if there are transactions
                    if (statement.Transactions.Any())
                    {
                        multiCurrencyStatement.CurrencyStatements.Add(statement);
                    }
                }

                _logger.LogDebug(
                    "Generated customer statement for customer {CustomerId} with {CurrencyCount} currencies and {TransactionCount} total transactions",
                    customer.Id,
                    multiCurrencyStatement.ActiveCurrencyCount,
                    multiCurrencyStatement.TotalTransactionCount);

                return Result<CustomerMultiCurrencyStatementDto>.Success(
                    multiCurrencyStatement,
                    "Müşteri hesap özeti başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating customer statement for customer {CustomerId}", request.CustomerId);
                return Result<CustomerMultiCurrencyStatementDto>.InternalError(
                    "Müşteri hesap özeti oluşturulurken beklenmeyen bir hata oluştu");
            }
        }

        private CustomerStatementDto BuildCurrencyStatement(
            IAMS.Domain.Entities.Customer customer,
            IAMS.Domain.Entities.Currency currency,
            List<IAMS.Domain.Entities.Policy> policies,
            List<IAMS.Domain.Entities.PolicyPayment> payments,
            DateTime startDate,
            DateTime endDate,
            string? customerAddress)
        {
            var transactions = new List<CustomerStatementTransactionDto>();

            // Add policy transactions (debits - money owed by customer)
            foreach (var policy in policies)
            {
                // Include policies that start before or during the period
                if (policy.StartDate <= endDate)
                {
                    var transactionDate = policy.StartDate;

                    // Determine transaction type based on StateType
                    var transactionType = policy.StateType switch
                    {
                        StateType.YeniPolice => "POL NO: PER",
                        StateType.Tecdit => "BORDRO: POL NO: PER",
                        StateType.Iptal => "BORDRO: POL NO: PER",
                        _ => "POL NO: PER"
                    };

                    var description = $"{policy.PolicyType?.Name ?? "POLİÇE"} - {policy.InsuranceCompany?.Name ?? ""}";

                    if (policy.InnerCode != "000")
                    {
                        description += $" (Zeyilname: {policy.InnerCode})";
                        transactionType = "BORDRO: POL NO: PER";
                    }

                    transactions.Add(new CustomerStatementTransactionDto
                    {
                        DocumentNumber = policy.PolicyNumber + (policy.InnerCode != "000" ? $"-{policy.InnerCode}" : ""),
                        TransactionDate = transactionDate,
                        Debit = policy.PremiumAmount, // Customer owes this amount
                        Credit = 0,
                        Description = description,
                        TransactionType = transactionType,
                        PolicyNumber = policy.PolicyNumber,
                        Balance = 0 // Will be calculated later
                    });
                }
            }

            // Add payment transactions (credits - money paid by customer)
            foreach (var payment in payments.Where(p => p.Status == PaymentStatus.Completed))
            {
                if (payment.PaymentDate >= startDate && payment.PaymentDate <= endDate)
                {
                    var policy = policies.FirstOrDefault(p => p.Id == payment.PolicyId);
                    var policyNumber = policy?.PolicyNumber ?? "";

                    var paymentMethodText = payment.PaymentMethod switch
                    {
                        PaymentMethod.Cash => "NAKİT",
                        PaymentMethod.CreditCard => "KREDİ KARTI",
                        PaymentMethod.BankTransfer => "BANKA HAVALESI",
                        PaymentMethod.Cheque => "ÇEK",
                        _ => payment.PaymentMethod.ToString().ToUpper()
                    };

                    transactions.Add(new CustomerStatementTransactionDto
                    {
                        DocumentNumber = payment.Reference ?? payment.Id.ToString(),
                        TransactionDate = payment.PaymentDate,
                        Debit = 0,
                        Credit = payment.Amount, // Customer paid this amount
                        Description = $"{paymentMethodText} - {policy?.PolicyType?.Name ?? "Ödeme"}",
                        TransactionType = "BORDRO: POL NO: PER",
                        PolicyNumber = policyNumber,
                        PaymentReference = payment.Reference,
                        Balance = 0 // Will be calculated later
                    });
                }
            }

            // Sort transactions by date
            transactions = transactions.OrderBy(t => t.TransactionDate).ThenBy(t => t.DocumentNumber).ToList();

            // Calculate opening balance (transactions before start date)
            var openingBalance = CalculateOpeningBalance(policies, payments, startDate);

            // Calculate running balances
            var runningBalance = openingBalance;
            foreach (var transaction in transactions)
            {
                runningBalance += transaction.Debit - transaction.Credit;
                transaction.Balance = runningBalance;
            }

            // Calculate totals
            var totalDebit = transactions.Sum(t => t.Debit);
            var totalCredit = transactions.Sum(t => t.Credit);
            var closingBalance = openingBalance + totalDebit - totalCredit;

            return new CustomerStatementDto
            {
                CustomerId = customer.Id,
                CustomerCode = customer.CustomerCode,
                CustomerName = $"{customer.FirstName} {customer.LastName}".Trim(),
                CustomerAddress = customerAddress,
                StartDate = startDate,
                EndDate = endDate,
                CurrencyCode = currency.Code,
                CurrencySymbol = currency.Symbol,
                CurrencyName = currency.Name ?? currency.Code,
                OpeningBalance = openingBalance,
                ClosingBalance = closingBalance,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                Transactions = transactions,
                GeneratedAt = DateTime.UtcNow,
                FileNumber = customer.CustomerCode
            };
        }

        private decimal CalculateOpeningBalance(
            List<IAMS.Domain.Entities.Policy> policies,
            List<IAMS.Domain.Entities.PolicyPayment> payments,
            DateTime startDate)
        {
            // Policies before start date (debits)
            var policiesBeforeStart = policies
                .Where(p => p.StartDate < startDate)
                .Sum(p => p.PremiumAmount);

            // Payments before start date (credits)
            var paymentsBeforeStart = payments
                .Where(p => p.Status == PaymentStatus.Completed && p.PaymentDate < startDate)
                .Sum(p => p.Amount);

            return policiesBeforeStart - paymentsBeforeStart;
        }

        private string? BuildCustomerAddress(IAMS.Domain.Entities.Customer customer)
        {
            var addressParts = new List<string>();

            if (!string.IsNullOrWhiteSpace(customer.Address1))
                addressParts.Add(customer.Address1);

            if (!string.IsNullOrWhiteSpace(customer.Address2))
                addressParts.Add(customer.Address2);

            // Note: The navigation properties for City, District, etc. might not be loaded
            // You may need to load them separately if you want to include them
            // For now, we'll just use the address fields that are available

            return addressParts.Any() ? string.Join(", ", addressParts) : null;
        }
    }
}
