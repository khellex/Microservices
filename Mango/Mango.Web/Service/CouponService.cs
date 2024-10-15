using Mango.Web.Models;
using Mango.Web.Service.IService;
using static Mango.Web.Utilities.StaticDetails;

namespace Mango.Web.Service
{
    public class CouponService : ICouponService
    {
        //injecting the IBaseService interface
        private readonly IBaseService _baseService;

        public CouponService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDTO?> CreateCouponsAsync(CouponDTO couponDTO)
        {
            return (await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = ApiType.POST,
                Data = couponDTO,
                Url = ApibaseURL + "/api/coupon"
            }));
        }

        public async Task<ResponseDTO?> DeleteCouponsAsync(int id)
        {
            return (await _baseService.SendAsync(new RequestDTO() 
            {
                ApiType = ApiType.DELETE,
                Url = ApibaseURL + "/api/coupon/" + id
            }));
        }

        public async Task<ResponseDTO?> GetAllCouponsAsync()
        {
            return (await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = ApiType.GET,
                Url = ApibaseURL + "/api/coupon"
            }));
        }

        public async Task<ResponseDTO?> GetCouponByCodeAsync(string code)
        {
            return (await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = ApiType.GET,
                Url = ApibaseURL + "/api/coupon/GetByCode/" + code
            }));
        }

        public async Task<ResponseDTO?> GetCouponByIdAsync(int id)
        {
            return (await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = ApiType.GET,
                Url = ApibaseURL + "/api/coupon/" + id
            }));
        }

        public async Task<ResponseDTO?> UpdateCouponsAsync(CouponDTO couponDTO)
        {
            return (await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = ApiType.PUT,
                Data = couponDTO,
                Url = ApibaseURL + "/api/coupon"
            }));
        }
    }
}
