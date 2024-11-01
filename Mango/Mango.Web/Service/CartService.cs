using Mango.Web.Models;
using Mango.Web.Service.IService;
using System;
using static Mango.Web.Utilities.StaticDetails;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Mango.Web.Service
{
    public class CartService : ICartService
    {
        private readonly IBaseService _baseService;
        public CartService(IBaseService baseService)
        {
            _baseService = baseService;
        }
        public async Task<ResponseDto?> ApplyCouponAsync(CartDto cartDto)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.POST,
                Data = cartDto,
                Url = CartApiBaseURL + "/api/cart/ApplyCoupon"
            }));
        }

        public async Task<ResponseDto?> CartUpsertAsync(CartDto cartDto)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.POST,
                Data = cartDto,
                Url = CartApiBaseURL + "/api/cart/CartUpsert"
            }));
        }

        public async Task<ResponseDto?> GetCartByUserId(string userId)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.GET,
                Url = CartApiBaseURL + "/api/cart/GetCart/" + userId
            }));
        }

        public async Task<ResponseDto?> RemoveFromCartAsync(int cartDetailsID)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.POST,
                Data = cartDetailsID,
                Url = CartApiBaseURL + "/api/cart/RemoveCart"
            }));
        }

        public async Task<ResponseDto?> RemoveCoupon(CartDto cartDto)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.POST,
                Data = cartDto,
                Url = CartApiBaseURL + "/api/cart/RemoveCoupon"
            }));
        }
    }
}
