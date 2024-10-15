using Mango.Web.Models;

namespace Mango.Web.Service.IService
{
    public interface ICouponService
    {
        public Task<ResponseDto?> GetCouponByCodeAsync(string code);
        public Task<ResponseDto?> GetCouponByIdAsync(int id);
        public Task<ResponseDto?> GetAllCouponsAsync();
        public Task<ResponseDto?> CreateCouponsAsync(CouponDto CouponDto);
        public Task<ResponseDto?> UpdateCouponsAsync(CouponDto CouponDto);
        public Task<ResponseDto?> DeleteCouponsAsync(int id);
    }
}
