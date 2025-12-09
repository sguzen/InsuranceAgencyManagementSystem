using AutoMapper;
using FluentValidation;
using IAMS.Shared.QueryParams;
using IAMS.Application.Features.Policies.Commands.UpdatePolicy;
using IAMS.Application.Features.Policies.Queries.GetMonthlyRevenue;
using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Application.Interfaces.Services;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IAMS.Application.Models;

namespace IAMS.Application.Features.Policies.Queries.GetRevenueByMonth
{
    public class GetRevenueByMonthQueryHandler : IRequestHandler<GetRevenueByMonthQuery, Result<Dictionary<string, decimal>>>
    {
        private readonly IPolicyAnalyticsService _policyAnalyticsService;
        private readonly ILogger<GetRevenueByMonthQueryHandler> _logger;
        public GetRevenueByMonthQueryHandler(
            IPolicyAnalyticsService policyAnalyticsService,
            ILogger<GetRevenueByMonthQueryHandler> logger)
    {
        _policyAnalyticsService = policyAnalyticsService;
        _logger = logger;
    }

    public async Task<Result<Dictionary<string, decimal>>> Handle(GetRevenueByMonthQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var revenueByMonth = await _policyAnalyticsService.GetRevenueByMonthAsync(request.Months);

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
