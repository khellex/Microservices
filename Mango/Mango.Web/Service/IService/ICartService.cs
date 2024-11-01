using Mango.Web.Models;

namespace Mango.Web.Service.IService
{
    public interface ICartService
    {
        public Task<ResponseDto?> CartUpsertAsync(CartDto cartDto);
        public Task<ResponseDto?> RemoveFromCartAsync(int cartDetailsID);
        public Task<ResponseDto?> GetCartByUserId(string userId);
        public Task<ResponseDto?> ApplyCouponAsync(CartDto cartDto);
        public Task<ResponseDto?> RemoveCoupon(CartDto cartDto);
    }
}
