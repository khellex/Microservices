using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utilities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        public IActionResult OrderIndex()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            IEnumerable<OrderHeaderDto> orders;
            string? userId = null;

            //if the user is NOT an admin, then we will fecth the userID,
            //backend code is written to handle the null userID 
            if (!User.IsInRole(StaticDetails.AdminRole))
            {
                userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            }

            ResponseDto? response = await _orderService.GetOrdersAsync(userId);

            if (response != null && response.IsSuccess)
            {
                orders = JsonConvert.DeserializeObject<List<OrderHeaderDto>>(Convert.ToString(response.Result));
            }
            else
            {
                orders = new List<OrderHeaderDto>();
            }
            return Json(new { data = orders });

        }
    }
}
