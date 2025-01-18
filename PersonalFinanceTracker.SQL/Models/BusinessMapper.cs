using AutoMapper;
using PersonalFinanceTracker.Model;

namespace PersonalFinanceTracker.SQL.Models
{
    public class BusinessMapper : Profile
    {
        public BusinessMapper()
        {
            CreateMap<Income, IncomeOut>()
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.EncryptedAmount));

            // Reverse mapping: from IncomeOut to Income
            CreateMap<IncomeOut, Income>()
                .ForMember(dest => dest.EncryptedAmount, opt => opt.MapFrom(src => src.Amount));

            CreateMap<Income, IncomeIn>()
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.EncryptedAmount));

            // Reverse mapping: from IncomeOut to Income
            CreateMap<IncomeIn, Income>()
                .ForMember(dest => dest.EncryptedAmount, opt => opt.MapFrom(src => src.Amount));
        }
    }
}
