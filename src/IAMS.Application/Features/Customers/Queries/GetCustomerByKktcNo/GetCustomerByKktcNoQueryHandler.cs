using AutoMapper;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerByKktcNo
{
    public class GetCustomerByKktcNoQueryHandler : IRequestHandler<GetCustomerByKktcNoQuery, Result<CustomerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCustomerByKktcNoQueryHandler> _logger;

        public GetCustomerByKktcNoQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCustomerByKktcNoQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CustomerDto>> Handle(GetCustomerByKktcNoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByKktcNoAsync(request.KktcNo, request.TentantId);
                if (customer == null)
                {
                    return Result<CustomerDto>.NotFound($"KKTC No '{request.KktcNo}' ile müşteri bulunamadı");
                }

                var customerDto = _mapper.Map<CustomerDto>(customer);
                return Result<CustomerDto>.Success(customerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by KKTC No: {KktcNo}", request.KktcNo);
                return Result<CustomerDto>.InternalError("Müşteri getirilirken beklenmeyen bir hata oluştu");
            }
        }
    }
}