using Mango.Web.Models;
using Mango.Web.Service.IService;
using static Mango.Web.Utilities.StaticDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    [Authorize]
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
        [HttpPost("UpdateOrderStatus")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus)
        {
            try
            {
                ResponseDto? response = new();

                if (!string.IsNullOrEmpty(newStatus) && orderId != null)
                {
                    response = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
                    if (response != null && response.IsSuccess)
                    {
                        TempData["success"] = response.Message;
                        return RedirectToAction("OrderDetails", new { orderId = orderId });
                    }
                }
                else
                {
                    TempData["error"] = "Something went wrong.";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return View();
        }

        #region AJAX calls
        /// <summary>
        /// Fetches and loads all the orders based on the logged in user
        /// if the user is NOT an admin, then we will fetch the userID,
        /// backend code is written to handle the null userID
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllOrders(string status)
        {
            IEnumerable<OrderHeaderDto> orders;
            string? userId = null;

            //if the user is NOT an admin, then we will fetch the userID,
            //backend code is written to handle the null userID 
            if (!User.IsInRole(AdminRole))
            {
                userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            }

            ResponseDto? response = await _orderService.GetOrdersAsync(userId);

            if (response != null && response.IsSuccess)
            {
                orders = JsonConvert.DeserializeObject<List<OrderHeaderDto>>(Convert.ToString(response.Result));
                if (!string.IsNullOrEmpty(status) && Statuses.ContainsValue(status))
                {
                    orders = orders.Where(o => o.Status == status).ToList();
                }
            }
            else
            {
                orders = new List<OrderHeaderDto>();
            }
            return Json(new { data = orders });
        }
        [ActionName("OrderDetails")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            try
            {
                OrderHeaderDto order;

                string? userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;

                if (orderId != null)
                {
                    
                    ResponseDto? response = await _orderService.GetOrderByIdAsync(orderId);

                    if (response != null && response.IsSuccess)
                    {
                       order = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));

                       if (!User.IsInRole(AdminRole) && userId != order.UserId)
                       {
                            return NotFound();
                       }
                       return View(order);
                    }
                    TempData["error"] = "Something went wrong.";
                }
                else
                {
                    TempData["error"] = "Something went wrong.";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return RedirectToAction(nameof(GetAllOrders));
        }
        #endregion
    }
}
