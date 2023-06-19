using AutoMapper;
using Finance.App.Domain.Models;
using Finance.App.Domain.Services.Communication;
using Finance.App.Dtos;

namespace Finance.App.Helpers.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<SaveCategoryDto, Category>();
            CreateMap<FinancialOperation, FinancialOperationDto>();
            CreateMap<SaveFinancialOperationDto, FinancialOperation>();
            CreateMap<DateOperationResponse, OperationsByDateDto>()
                .ForMember(dest => dest.Income, opt => opt.MapFrom(src => src.Income))
                .ForMember(dest => dest.Expenses, opt => opt.MapFrom(src => src.Expenses))
                .ForMember(dest => dest.Operations, opt => opt.Ignore());
        }
    }
}
