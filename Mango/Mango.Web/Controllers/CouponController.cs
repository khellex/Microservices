using Mango.Web.Models;
using Mango.Web.Service;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class CouponController : Controller
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }
        #region Coupon Services
        /// <summary>
        /// Fetches the full list of coupons using the 
        /// GetAllCouponsAsync() api endpoint
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CouponIndex()
        {
            List<CouponDto> coupons = new();

            ResponseDto? response = await _couponService.GetAllCouponsAsync();
            if (response != null && response.IsSuccess)
            {
                coupons = JsonConvert.DeserializeObject<List<CouponDto>>(Convert.ToString(response.Result));
            }
            TempData["information"] = "Coupon list loaded successfully!";
            return View(coupons);
        }
        /// <summary>
        /// Used to load the initial create Coupon page view
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> CreateCoupon()
        {
            return View();
        }
        /// <summary>
        /// Used to create new coupon by calling the 
        /// CreateCouponsAsync() api endpoint
        /// </summary>
        /// <param name="CouponDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateCoupon(CouponDto CouponDto)
        {
            if (ModelState.IsValid)
            {
                ResponseDto? response = await _couponService.CreateCouponsAsync(CouponDto);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Coupon Created Successfully";
                    return RedirectToAction(nameof(CouponIndex));
                    
                }
            }
            TempData["error"] = "Something went wrong";
            return View(CouponDto);
        }
        /// <summary>
        /// Used to load the initial Delete coupon page
        /// </summary>
        /// <param name="couponId"></param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteCoupon(int couponId)
        {
            ResponseDto? response = await _couponService.GetCouponByIdAsync(couponId);
            if (response != null && response.IsSuccess)
            {
                CouponDto? coupon = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(response.Result));
                TempData["information"] = "Page loaded Successfully";
                return View(coupon);
            }
            TempData["error"] = "Something went wrong";
            return NotFound();
        }
        /// <summary>
        /// Used to delete a coupon from the listing using the 
        /// DeleteCouponsAsync() api endpoint and then redirect
        /// the page to the Coupon index page.
        /// </summary>
        /// <param name="CouponDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteCoupon(CouponDto CouponDto)
        {
            var deleteCoupon = await _couponService.DeleteCouponsAsync(CouponDto.CouponId);

            ResponseDto? response = await _couponService.GetAllCouponsAsync();

            if (deleteCoupon != null && deleteCoupon.IsSuccess)
            {
                TempData["success"] = "Coupon Deleted Successfully";
                return RedirectToAction(nameof(CouponIndex), response);
            }
            return RedirectToAction(nameof(DeleteCoupon), CouponDto.CouponId );
        }
        #endregion
    }
}
