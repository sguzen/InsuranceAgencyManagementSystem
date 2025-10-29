using AutoMapper;
using FluentValidation;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Features.Policies.Commands.UpdatePolicy;
using IAMS.Application.Features.Policies.Queries.GetMonthlyRevenue;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.Features.Policies.Queries.GetRevenueByMonth
{
    public class GetRevenueByMonthQueryHandler : IRequestHandler<GetRevenueByMonthQuery, Result<Dictionary<string, decimal>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetRevenueByMonthQueryHandler> _logger;
        public GetRevenueByMonthQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetRevenueByMonthQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Dictionary<string, decimal>>> Handle(GetRevenueByMonthQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var revenueByMonth = await _unitOfWork.Policies.GetRevenueByMonthAsync(request.Months);

            return Result<Dictionary<string, decimal>>.Success(revenueByMonth, $"Son {request.Months} ayın gelir verileri");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving revenue by month");
            return Result<Dictionary<string, decimal>>.InternalError("Aylık gelir verileri getirilirken beklenmeyen bir hata oluştu");
        }
    }
}
}
