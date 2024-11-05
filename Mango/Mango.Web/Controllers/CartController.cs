using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }
        [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            return View(await LoadCartDtoBasedOnLoggedInUser());
        }
        private async Task<CartDto> LoadCartDtoBasedOnLoggedInUser()
        {
            var loggedInUserId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            if (!string.IsNullOrEmpty(loggedInUserId))
            {
                ResponseDto? userCartResponse = await _cartService.GetCartByUserId(loggedInUserId);
                if (userCartResponse.Result != null && userCartResponse.IsSuccess)
                {
                    CartDto cart = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(userCartResponse.Result));
                    return cart;
                }
                return new();
            }
            else
            {
                return new();
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
        {
            if (!string.IsNullOrEmpty(cartDto.CartHeaderDto.UserId))
            {
                ResponseDto? applyCouponResponse = await _cartService.ApplyCouponAsync(cartDto);
                if (applyCouponResponse.Result != null && applyCouponResponse.IsSuccess)
                {
                    TempData["success"]=applyCouponResponse.Message;
                    return RedirectToAction(nameof(CartIndex));
                }
                TempData["error"] = applyCouponResponse.Message;
                return RedirectToAction(nameof(CartIndex));
            }
            else
            {
                TempData["warning"] = "Please login with user credentials";
                return RedirectToAction(nameof(CartIndex));
            }
        }
        [Authorize]
        public async Task<IActionResult> RemoveItem(int cartDetailsID)
        {
            var loggedInUserId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;

            if (!string.IsNullOrEmpty(loggedInUserId))
            {
                ResponseDto? removeItemResponse = await _cartService.RemoveFromCartAsync(cartDetailsID);
                if (removeItemResponse.Result != null && removeItemResponse.IsSuccess)
                {
                    TempData["success"] = removeItemResponse.Message;
                    return RedirectToAction(nameof(CartIndex));
                }
                TempData["error"] = removeItemResponse.Message;
                return RedirectToAction(nameof(CartIndex));
            }
            else
            {
                TempData["warning"] = "Please login with user credentials";
                return RedirectToAction(nameof(CartIndex));
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
        {
            if (!string.IsNullOrEmpty(cartDto.CartHeaderDto.UserId))
            {
                ResponseDto? removeCouponResponse = await _cartService.RemoveCoupon(cartDto);
                if (removeCouponResponse.Result != null && removeCouponResponse.IsSuccess)
                {
                    TempData["success"] = removeCouponResponse.Message;
                    return RedirectToAction(nameof(CartIndex));
                }
                TempData["error"] = removeCouponResponse.Message;
                return RedirectToAction(nameof(CartIndex));
            }
            else
            {
                TempData["warning"] = "Please login with user credentials";
                return RedirectToAction(nameof(CartIndex));
            }
        }
        [HttpPost]
        public async Task<IActionResult> EmailCart(CartDto cartDto)
        {
            CartDto cart = await LoadCartDtoBasedOnLoggedInUser();
            cart.CartHeaderDto.Email = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault()?.Value;
            ResponseDto? emailResponse = await _cartService.EmailCart(cart);
            if (emailResponse.Result != null && emailResponse.IsSuccess)
            {
                TempData["success"] = emailResponse.Message;
                return RedirectToAction(nameof(CartIndex));
            }
            TempData["error"] = emailResponse.Message;
            return RedirectToAction(nameof(CartIndex));
        }
    }
}
