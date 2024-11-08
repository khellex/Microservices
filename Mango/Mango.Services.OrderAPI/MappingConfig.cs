using AutoMapper;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Mango.Services.OrderAPI
{
    /// <summary>
    /// This is the config setup for the Auto mapper,
    /// for automating the mapping between different
    /// object models. By reducing repetitive code, 
    /// enhancing maintainability, and supporting complex 
    /// mapping scenarios.
    /// </summary>
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                //here we set a mapping from cartheaderdto to orderheaderdto,
                //where we map the OrderTotal property from OrderHeaderDto to
                //the CartTotal property from CartHeaderDto
                config.CreateMap<CartHeaderDto, OrderHeaderDto>().ForMember(dest => dest.OrderTotal, u => u.MapFrom(src => src.CartTotal)).ReverseMap();

                //here we set a mapping from cartdetailsdto to orderdetailsdto,
                //the reverse mapping of this is not required, but we need the
                //orderdetailsdto mappes to cartdetailsdto and hence we have 
                //added it in the next line
                config.CreateMap<CartDetailsDto, OrderDetailsDto>()
                .ForMember(dest => dest.ProductName, u => u.MapFrom(src => src.ProductDto.Name))
                .ForMember(dest => dest.Price, u => u.MapFrom(src => src.ProductDto.Price));

                config.CreateMap<OrderDetailsDto, CartDetailsDto>();

                //CreateMap, maps OrderHeader to OrderHeaderDto and vice versa
                //Instead of writing the other way round, we can use ReverseMap()
                config.CreateMap<OrderHeader, OrderHeaderDto>().ReverseMap();
                config.CreateMap<OrderDetails, OrderDetailsDto>().ReverseMap();
            });
            return mappingConfig;
        }
    }
}
