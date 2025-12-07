using IAMS.Application.DTOs.Vehicle;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Queries.GetActiveModelsByBrandId
{
    public class GetActiveModelsByBrandIdQuery : IRequest<Result<List<VehicleModelDto>>>
    {
        public int BrandId { get; set; }

        public GetActiveModelsByBrandIdQuery(int brandId)
        {
            BrandId = brandId;
        }
    }
}
