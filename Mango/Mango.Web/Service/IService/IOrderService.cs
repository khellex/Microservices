using Mango.Web.Models;

namespace Mango.Web.Service.IService
{
    public interface IOrderService
    {
        public Task<ResponseDto?> CreateOrderAsync(CartDto cartDto);
    }
}
