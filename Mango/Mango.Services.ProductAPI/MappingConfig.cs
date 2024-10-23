using AutoMapper;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;

namespace Mango.Services.CouponAPI
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
                config.CreateMap<ProductDto, Product>();
                config.CreateMap<Product, ProductDto>();
            });
            return mappingConfig;
        }
    }
}
