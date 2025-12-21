using AutoMapper;
using IAMS.Shared.DTOs.Invoice;
using IAMS.Domain.Entities;

namespace IAMS.Application.Mappings
{
    public class InvoiceMappingProfile : Profile
    {
        public InvoiceMappingProfile()
        {
            CreateMap<Invoice, InvoiceDto>()
                .ForMember(dest => dest.PolicyNumber, opt => opt.MapFrom(src => src.Policy.PolicyNumber))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                    src.Customer.Type == Domain.Enums.CustomerType.Individual
                        ? $"{src.Customer.FirstName} {src.Customer.LastName}"
                        : src.Customer.FirstName))
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.Currency.Code))
                .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src => src.IsOverdue))
                .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.IsPaid))
                .ForMember(dest => dest.DaysOverdue, opt => opt.MapFrom(src => src.DaysOverdue))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.InvoiceItems));

            CreateMap<CreateOrUpdateInvoiceDto, Invoice>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SubtotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TaxAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentDate, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReference, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Policy, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceItems, opt => opt.Ignore());

            CreateMap<InvoiceItem, InvoiceItemDto>();

            CreateMap<CreateOrUpdateInvoiceItemDto, InvoiceItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore());
        }
    }
}
