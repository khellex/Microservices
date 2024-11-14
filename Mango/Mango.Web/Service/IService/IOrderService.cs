using Mango.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Web.Service.IService
{
    public interface IOrderService
    {
        public Task<ResponseDto?> CreateOrderAsync(CartDto cartDto);
        public Task<ResponseDto?> CreateStripeSessionAsync(StripeRequestDto stripeRequestDto);
        public Task<ResponseDto?> ValidateStripeSessionAsync(int orderId);
        public Task<ResponseDto?> GetOrdersAsync(string? userId);
        public Task<ResponseDto?> GetOrderByIdAsync(int orderId);
        public Task<ResponseDto?> UpdateOrderStatusAsync(int orderId, string newStatus);
    }
}
