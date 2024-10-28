using AutoMapper;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;

namespace Mango.Services.ShoppingCartAPI
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
                //CreateMap, maps ProductDto to Product
                //Instead of writing the other way round, we can use ReverseMap()
                config.CreateMap<CartDetails, CartDetailsDto>().ReverseMap();
                config.CreateMap<CartHeader, CartHeaderDto>().ReverseMap();
            });
            return mappingConfig;
        }
    }
}
