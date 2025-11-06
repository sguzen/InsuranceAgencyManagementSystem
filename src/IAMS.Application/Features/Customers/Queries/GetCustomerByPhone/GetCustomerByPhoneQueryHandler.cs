using AutoMapper;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerByPhone
{
    public class GetCustomerByPhoneQueryHandler : IRequestHandler<GetCustomerByPhoneQuery, Result<CustomerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCustomerByPhoneQueryHandler> _logger;

        public GetCustomerByPhoneQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCustomerByPhoneQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CustomerDto>> Handle(GetCustomerByPhoneQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByPhoneAsync(request.Phone);
                if (customer == null)
                {
                    return Result<CustomerDto>.NotFound($"Telefon numarası '{request.Phone}' ile müşteri bulunamadı");
                }

                var customerDto = _mapper.Map<CustomerDto>(customer);
                return Result<CustomerDto>.Success(customerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by phone: {Phone}", request.Phone);
                return Result<CustomerDto>.InternalError("Müşteri getirilirken beklenmeyen bir hata oluştu");
            }
        }
    }
}