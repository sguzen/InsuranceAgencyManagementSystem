using AutoMapper;
using IAMS.Shared.QueryParams;
using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerByIdentificationNo
{
    public class GetCustomerByIdentificationNoQueryHandler : IRequestHandler<GetCustomerByIdentificationNoQuery, Result<CustomerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCustomerByIdentificationNoQueryHandler> _logger;

        public GetCustomerByIdentificationNoQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCustomerByIdentificationNoQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CustomerDto>> Handle(GetCustomerByIdentificationNoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdentificationNoAsync(request.IdentificationNo);
                if (customer == null)
                {
                    return Result<CustomerDto>.NotFound($"KKTC No '{request.IdentificationNo}' ile müşteri bulunamadı");
                }

                var customerDto = _mapper.Map<CustomerDto>(customer);
                return Result<CustomerDto>.Success(customerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by KKTC No: {IdentificationNo}", request.IdentificationNo);
                return Result<CustomerDto>.InternalError("Müşteri getirilirken beklenmeyen bir hata oluştu");
            }
        }
    }
}