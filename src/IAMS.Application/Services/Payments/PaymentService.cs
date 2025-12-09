using IAMS.Application.DTOs.Payment;
using IAMS.Application.Features.Payments.Commands.CreatePayment;
using IAMS.Application.Features.Payments.Commands.DeletePayment;
using IAMS.Application.Features.Payments.Commands.UpdatePayment;
using IAMS.Application.Features.Payments.Commands.UpdatePaymentStatus;
using IAMS.Application.Features.Payments.Queries.GetOverduePayments;
using IAMS.Application.Features.Payments.Queries.GetPaymentById;
using IAMS.Application.Features.Payments.Queries.GetPaymentsByPolicyId;
using IAMS.Application.Features.Payments.Queries.GetPaymentsPaged;
using IAMS.Application.Features.Payments.Queries.GetTotalPaymentsByPolicyId;
using IAMS.Application.Features.Payments.Queries.GetPaymentsDueThisMonth;
using IAMS.Application.Features.Payments.Queries.GetCustomersWithOutstandingBalance;
using IAMS.Application.Features.Payments.Queries.GetTotalOutstandingBalanceByCustomerId;
using IAMS.Shared.Models;
using IAMS.Domain.Enums;
using MediatR;

namespace IAMS.Application.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly IMediator _mediator;

        public PaymentService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<List<PolicyPaymentDto>> GetPaymentsByPolicyIdAsync(int policyId)
        {
            return await _mediator.Send(new GetPaymentsByPolicyIdQuery(policyId));
        }

        public async Task<PolicyPaymentDto?> GetByIdAsync(int id)
        {
            return await _mediator.Send(new GetPaymentByIdQuery(id));
        }

        public async Task<PolicyPaymentDto> CreateAsync(CreatePolicyPaymentDto paymentDto)
        {
            var result = await _mediator.Send(new CreatePaymentCommand(paymentDto));
            if (!result.IsSuccess || result.Data == null)
            {
                throw new InvalidOperationException(result.Message);
            }
            return result.Data;
        }

        public async Task UpdateAsync(UpdatePolicyPaymentDto paymentDto)
        {
            var result = await _mediator.Send(new UpdatePaymentCommand(paymentDto));
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Message);
            }
        }

        public async Task UpdatePaymentStatusAsync(int id, PaymentStatus status)
        {
            var result = await _mediator.Send(new UpdatePaymentStatusCommand(id, status));
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Message);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var result = await _mediator.Send(new DeletePaymentCommand(id));
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Message);
            }
        }

        public async Task<List<PolicyPaymentDto>> GetOverduePaymentsAsync()
        {
            return await _mediator.Send(new GetOverduePaymentsQuery());
        }

        public async Task<PagedResult<PolicyPaymentDto>> GetPaymentsPagedAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            return await _mediator.Send(new GetPaymentsPagedQuery(pageNumber, pageSize, searchTerm));
        }

        public async Task<decimal> GetTotalPaymentsByPolicyIdAsync(int policyId)
        {
            return await _mediator.Send(new GetTotalPaymentsByPolicyIdQuery(policyId));
        }

        public async Task<List<PolicyPaymentDto>> GetPaymentsDueThisMonthAsync()
        {
            return await _mediator.Send(new GetPaymentsDueThisMonthQuery());
        }

        public async Task<List<CustomerOutstandingBalanceDto>> GetCustomersWithOutstandingBalanceAsync()
        {
            return await _mediator.Send(new GetCustomersWithOutstandingBalanceQuery());
        }

        public async Task<decimal> GetTotalOutstandingBalanceByCustomerIdAsync(int customerId)
        {
            return await _mediator.Send(new GetTotalOutstandingBalanceByCustomerIdQuery(customerId));
        }
    }
}
