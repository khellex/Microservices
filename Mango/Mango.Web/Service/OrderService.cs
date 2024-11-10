using Mango.Web.Models;
using Mango.Web.Service.IService;
using static Mango.Web.Utilities.StaticDetails;

namespace Mango.Web.Service
{
    public class OrderService : IOrderService
    {
        //injecting the IBaseService interface
        private readonly IBaseService _baseService;

        public OrderService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> CreateOrderAsync(CartDto cartDto)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.POST,
                Data = cartDto,
                Url = OrderApiBaseURL + "/api/order/CreateOrder"
            }));
        }

        public async Task<ResponseDto?> CreateStripeSessionAsync(StripeRequestDto stripeRequestDto)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.POST,
                Data = stripeRequestDto,
                Url = OrderApiBaseURL + "/api/order/CreateStripeSession"
            }));
        }
    }
}
