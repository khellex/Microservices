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

        public async Task<ResponseDto?> CreateCouponsAsync(CouponDto CouponDto)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.POST,
                Data = CouponDto,
                Url = ApibaseURL + "/api/coupon"
            }));
        }

        public async Task<ResponseDto?> DeleteCouponsAsync(int id)
        {
            return (await _baseService.SendAsync(new RequestDto() 
            {
                ApiType = ApiType.DELETE,
                Url = ApibaseURL + "/api/coupon/" + id
            }));
        }

        public async Task<ResponseDto?> GetAllCouponsAsync()
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.GET,
                Url = ApibaseURL + "/api/coupon"
            }));
        }

        public async Task<ResponseDto?> GetCouponByCodeAsync(string code)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.GET,
                Url = ApibaseURL + "/api/coupon/GetByCode/" + code
            }));
        }

        public async Task<ResponseDto?> GetCouponByIdAsync(int id)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.GET,
                Url = ApibaseURL + "/api/coupon/" + id
            }));
        }

        public async Task<ResponseDto?> UpdateCouponsAsync(CouponDto CouponDto)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.PUT,
                Data = CouponDto,
                Url = ApibaseURL + "/api/coupon"
            }));
        }
    }
}
