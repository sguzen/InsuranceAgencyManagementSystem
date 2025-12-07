using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Payments.Commands.UpdatePaymentStatus
{
    public class UpdatePaymentStatusCommandHandler : IRequestHandler<UpdatePaymentStatusCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdatePaymentStatusCommandHandler> _logger;

        public UpdatePaymentStatusCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpdatePaymentStatusCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdatePaymentStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var payment = await _unitOfWork.PolicyPayments.GetByIdAsync(request.Id);
                if (payment == null)
                {
                    return Result.NotFound($"Payment with ID {request.Id} not found");
                }

                payment.Status = request.Status;
                payment.ModifiedOn = DateTime.UtcNow;

                _unitOfWork.PolicyPayments.Update(payment);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Payment status updated successfully for ID: {PaymentId}", request.Id);
                return Result.Success("Payment status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for ID: {PaymentId}", request.Id);
                return Result.InternalError("Error updating payment status", new List<string> { ex.Message });
            }
        }
    }
}
