using AutoMapper;
using IAMS.Application.Models;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Payments.Commands.UpdatePayment
{
    public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdatePaymentCommandHandler> _logger;

        public UpdatePaymentCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpdatePaymentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get existing payment
                var payment = await _unitOfWork.PolicyPayments.GetByIdAsync(request.PaymentDto.Id);
                if (payment == null)
                {
                    return Result.NotFound($"Payment with ID {request.PaymentDto.Id} not found");
                }

                // Update properties
                payment.Amount = request.PaymentDto.Amount;
                payment.PaymentDate = request.PaymentDto.PaymentDate;
                payment.DueDate = request.PaymentDto.DueDate;
                payment.PaymentMethod = request.PaymentDto.PaymentMethod;
                payment.Status = request.PaymentDto.Status;
                payment.Notes = request.PaymentDto.Notes;
                payment.Reference = request.PaymentDto.Reference;
                payment.ModifiedOn = DateTime.UtcNow;

                _unitOfWork.PolicyPayments.Update(payment);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Payment updated successfully with ID: {PaymentId}", payment.Id);

                return Result.Success("Payment updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment with ID: {PaymentId}", request.PaymentDto.Id);
                return Result.InternalError("Error updating payment", new List<string> { ex.Message });
            }
        }
    }
}
