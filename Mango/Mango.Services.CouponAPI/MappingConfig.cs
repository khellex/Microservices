using AutoMapper;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.DTO;

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
        public static MapperConfiguration RegisterMaps() { 
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<CouponDto, Coupon>();
                config.CreateMap<Coupon, CouponDto>();
            });
            return mappingConfig;
        }
    }
}
