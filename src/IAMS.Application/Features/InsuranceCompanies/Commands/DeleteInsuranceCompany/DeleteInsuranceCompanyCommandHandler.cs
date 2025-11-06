using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.InsuranceCompanies.Commands.DeleteInsuranceCompany
{
    public class DeleteInsuranceCompanyCommandHandler : IRequestHandler<DeleteInsuranceCompanyCommand, Result>
    {
        private readonly IInsuranceCompanyRepository _insuranceCompanyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteInsuranceCompanyCommandHandler> _logger;

        public DeleteInsuranceCompanyCommandHandler(
            IInsuranceCompanyRepository insuranceCompanyRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeleteInsuranceCompanyCommandHandler> logger)
        {
            _insuranceCompanyRepository = insuranceCompanyRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(DeleteInsuranceCompanyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting insurance company with ID: {CompanyId}", request.Id);

                // Get existing company
                var company = await _insuranceCompanyRepository.GetByIdAsync(request.Id);
                if (company == null)
                {
                    return Result.NotFound($"Insurance company with ID {request.Id} not found");
                }

                // Check if company has active policies
                var activePolicies = company.Policies?.Any(p => p.Status == Domain.Enums.PolicyStatus.Active && !p.IsDeleted) ?? false;
                if (activePolicies)
                {
                    return Result.Failure("Cannot delete insurance company with active policies");
                }

                // Soft delete
                company.MarkAsDeleted("System"); // TODO: Get from current user context

                _insuranceCompanyRepository.Update(company);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Insurance company deleted successfully: {CompanyId}", company.Id);

                return Result.Success("Insurance company deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting insurance company with ID: {CompanyId}", request.Id);
                return Result.InternalError("An error occurred while deleting the insurance company");
            }
        }
    }
}
