using AutoMapper;
using IAMS.Domain.Entities;
using IAMS.Shared.DTOs.ImportConfiguration;

namespace IAMS.Application.Mappings
{
    public class ImportConfigurationMappingProfile : Profile
    {
        public ImportConfigurationMappingProfile()
        {
            CreateMap<ImportConfiguration, ImportConfigurationDto>()
                .ForMember(dest => dest.InsuranceCompanyName,
                    opt => opt.MapFrom(src => src.InsuranceCompany != null ? src.InsuranceCompany.Name : null));

            CreateMap<CreateImportConfigurationDto, ImportConfiguration>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.InsuranceCompany, opt => opt.Ignore())
                .ForMember(dest => dest.ImportHistories, opt => opt.Ignore())
                .ForMember(dest => dest.LastSyncDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastSyncStatus, opt => opt.Ignore());
        }
    }
}
