using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Commands.DeleteCustomer
{
    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteCustomerCommandHandler> _logger;

        public DeleteCustomerCommandHandler(
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeleteCustomerCommandHandler> logger)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting customer with ID: {CustomerId}", request.Id);

                var customer = await _customerRepository.GetByIdAsync(request.Id);
                if (customer == null)
                {
                    return Result.NotFound("Customer not found");
                }

                // Check if customer has active policies
                var activePolicies = await _customerRepository.GetActivePoliciesAsync(request.Id);
                if (activePolicies.Any())
                {
                    return Result.Failure("Cannot delete customer with active policies");
                }

                // Soft delete
                customer.Activate("System"); // TODO: Get from current user context
                _customerRepository.Update(customer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Customer deleted successfully: {CustomerId}", request.Id);

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