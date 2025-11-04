using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Queries.GetModelsByBrandId
{
    public class GetModelsByBrandIdQuery : IRequest<Result<List<VehicleModelDto>>>
    {
        public int BrandId { get; set; }

        public GetModelsByBrandIdQuery(int brandId)
        {
            BrandId = brandId;
        }
    }
}
