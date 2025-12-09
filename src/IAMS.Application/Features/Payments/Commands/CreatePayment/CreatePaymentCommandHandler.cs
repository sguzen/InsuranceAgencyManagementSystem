using AutoMapper;
using IAMS.Application.DTOs.Payment;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using IAMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using IAMS.Application.Models;

namespace IAMS.Application.Features.Payments.Commands.CreatePayment
{
    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<PolicyPaymentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreatePaymentCommandHandler> _logger;

        public CreatePaymentCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreatePaymentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PolicyPaymentDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verify policy exists
                var policy = await _unitOfWork.Policies.GetByIdAsync(request.PaymentDto.PolicyId);
                if (policy == null)
                {
                    return Result<PolicyPaymentDto>.NotFound($"Policy with ID {request.PaymentDto.PolicyId} not found");
                }

                var payment = _mapper.Map<PolicyPayment>(request.PaymentDto);
                payment.CreatedOn = DateTime.UtcNow;

                await _unitOfWork.PolicyPayments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Payment created successfully with ID: {PaymentId}", payment.Id);

                var paymentDto = _mapper.Map<PolicyPaymentDto>(payment);
                return Result<PolicyPaymentDto>.Success(paymentDto, "Payment created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment for policy ID: {PolicyId}", request.PaymentDto.PolicyId);
                return Result<PolicyPaymentDto>.InternalError("Error creating payment", new List<string> { ex.Message });
            }
        }
    }
}
