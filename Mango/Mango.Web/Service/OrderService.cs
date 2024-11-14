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

        public async Task<ResponseDto?> GetOrderByIdAsync(int orderId)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.GET,
                Url = OrderApiBaseURL + "/api/order/GetOrder/" + orderId
            }));
        }

        public async Task<ResponseDto?> GetOrdersAsync(string? userId)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.GET,
                Url = OrderApiBaseURL + "/api/order/GetOrders?userId=" + userId
            }));
        }

        public async Task<ResponseDto?> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.POST,
                Data = newStatus,
                Url = OrderApiBaseURL + "/api/order/UpdateOrderStatus/" + orderId
            }));
        }

        public async Task<ResponseDto?> ValidateStripeSessionAsync(int orderId)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.POST,
                Data = orderId,
                Url = OrderApiBaseURL + "/api/order/ValidateStripeSession"
            }));
        }
    }
}
