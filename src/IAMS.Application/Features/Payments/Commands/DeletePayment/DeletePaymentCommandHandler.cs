using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Payments.Commands.DeletePayment
{
    public class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeletePaymentCommandHandler> _logger;

        public DeletePaymentCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeletePaymentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var payment = await _unitOfWork.PolicyPayments.GetByIdAsync(request.Id);
                if (payment == null)
                {
                    return Result.NotFound($"Payment with ID {request.Id} not found");
                }

                _unitOfWork.PolicyPayments.Remove(payment);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Payment deleted successfully with ID: {PaymentId}", request.Id);
                return Result.Success("Payment deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment with ID: {PaymentId}", request.Id);
                return Result.InternalError("Error deleting payment", new List<string> { ex.Message });
            }
        }
    }
}
