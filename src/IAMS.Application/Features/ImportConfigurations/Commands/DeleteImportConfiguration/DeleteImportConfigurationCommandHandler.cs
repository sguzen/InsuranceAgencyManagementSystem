using IAMS.Application.Interfaces;
using IAMS.Application.Models;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.ImportConfigurations.Commands.DeleteImportConfiguration
{
    public class DeleteImportConfigurationCommandHandler
        : IRequestHandler<DeleteImportConfigurationCommand, Result>
    {
        private readonly IImportConfigurationRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteImportConfigurationCommandHandler> _logger;

        public DeleteImportConfigurationCommandHandler(
            IImportConfigurationRepository repository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<DeleteImportConfigurationCommandHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result> Handle(
            DeleteImportConfigurationCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting import configuration with ID: {Id}", request.Id);

                var configuration = await _repository.GetByIdAsync(request.Id);
                if (configuration == null)
                {
                    return Result.NotFound("Import konfigürasyonu bulunamadı");
                }

                // Soft delete
                configuration.MarkAsDeleted(_currentUserService.UserName ?? "System");

                _repository.Update(configuration);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Import configuration deleted: {Id}", configuration.Id);

                return Result.Success("Import konfigürasyonu başarıyla silindi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting import configuration with ID: {Id}", request.Id);
                return Result.InternalError("Import konfigürasyonu silinirken bir hata oluştu");
            }
        }
    }
}
