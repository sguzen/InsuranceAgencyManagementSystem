using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Commands.DeleteCustomer
{
    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteCustomerCommandHandler> _logger;

        public DeleteCustomerCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteCustomerCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id);
                if (customer == null)
                {
                    return Result.NotFound("Customer not found");
                }

                // Check if customer has any policies
                var customerPolicies = await _unitOfWork.Policies.GetPoliciesByCustomerIdAsync(request.Id);
                if (customerPolicies.Any())
                {
                    return Result.InternalError("Cannot delete customer with existing policies");
                }

                _unitOfWork.Customers.Remove(customer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Customer deleted successfully with ID: {CustomerId}", request.Id);
                return Result.Success("Customer deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer with ID: {CustomerId}", request.Id);
                return Result.InternalError("An error occurred while deleting the customer");
            }
        }
    }
}