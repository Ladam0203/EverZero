using AutoMapper;
using InvoiceService.Core;
using Domain;
using InvoiceService.Core.DTOs;

namespace InvoiceService.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapping for Invoice to InvoiceDTO
        CreateMap<Invoice, InvoiceDTO>()
            .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.Lines))
            .ReverseMap();
        
        // Mapping for InvoiceLine to InvoiceLineDTO
        CreateMap<InvoiceLine, InvoiceLineDTO>()
            .ReverseMap();
        
        // Mapping for PostInvoiceDTO to Invoice
        CreateMap<PostInvoiceDTO, Invoice>()
            .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.Lines))
            .ReverseMap();
        
        // Mapping for PostInvoiceLineDTO to InvoiceLine
        CreateMap<PostInvoiceLineDTO, InvoiceLine>()
            .ReverseMap();
    }
}