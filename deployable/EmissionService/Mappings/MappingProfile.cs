using AutoMapper;
using Domain;

namespace EmissionService.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapping for Invoice to InvoiceDTO
        CreateMap<InvoiceDTO, InvoiceCalculationDTO>()
            .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.Lines))
            .ReverseMap();
        
        // Mapping for InvoiceLine to InvoiceLineDTO
        CreateMap<InvoiceLineDTO, InvoiceLineCalculationDTO>()
            .ReverseMap();
    }
}