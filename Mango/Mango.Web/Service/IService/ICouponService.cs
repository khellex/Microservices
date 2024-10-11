using Mango.Web.Models;

namespace Mango.Web.Service.IService
{
    public interface ICouponService
    {
        public Task<ResponseDTO?> GetCouponByCodeAsync(string code);
        public Task<ResponseDTO?> GetCouponByIdAsync(int id);
        public Task<ResponseDTO?> GetAllCouponsAsync();
        public Task<ResponseDTO?> CreateCouponsAsync(CouponDTO couponDTO);
        public Task<ResponseDTO?> UpdateCouponsAsync(CouponDTO couponDTO);
        public Task<ResponseDTO?> DeleteCouponsAsync(int id);
    }
}
