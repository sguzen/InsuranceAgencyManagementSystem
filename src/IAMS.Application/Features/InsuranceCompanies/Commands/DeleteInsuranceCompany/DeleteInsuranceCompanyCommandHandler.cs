using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.InsuranceCompanies.Commands.DeleteInsuranceCompany
{
    public class DeleteInsuranceCompanyCommandHandler : IRequestHandler<DeleteInsuranceCompanyCommand, Result>
    {
        private readonly IInsuranceCompanyRepository _insuranceCompanyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteInsuranceCompanyCommandHandler> _logger;

        public DeleteInsuranceCompanyCommandHandler(
            IInsuranceCompanyRepository insuranceCompanyRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<DeleteInsuranceCompanyCommandHandler> logger)
        {
            _insuranceCompanyRepository = insuranceCompanyRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
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
                    return Result.NotFound("Sigorta şirketi bulunamadı");
                }

                // Check if company has active policies
                var activePolicies = company.Policies?.Any(p => p.Status == Domain.Enums.PolicyStatus.Active && !p.IsDeleted) ?? false;
                if (activePolicies)
                {
                    return Result.Failure("Sigorta şirketi aktif poliçeleri olduğu için silinemez", string.Empty);
                }

                // Soft delete
                company.MarkAsDeleted(_currentUserService.UserName ?? "System");

                _insuranceCompanyRepository.Update(company);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Insurance company deleted successfully: {CompanyId}", company.Id);

                return Result.Success("Sigorta şirketi başarıyla silindi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting insurance company with ID: {CompanyId}", request.Id);
                return Result.InternalError("Sigorta şirketi silinirken bir hata oluştu");
            }
        }
    }
}
