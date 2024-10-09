using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.CouponAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CouponAPIController(ApplicationDbContext dbContext)
        {
            _db = dbContext;
        }
        [HttpGet]
        public object Get() {
            try
            {
                IEnumerable<Coupon> objListOfCoupons = _db.Coupons.ToList();
                return objListOfCoupons;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }
        }
        [HttpGet(/*"{id:int}"*/)]
        [Route("{id:int}")]
        public async Task<object> Get(int id)
        {
            try
            {
                Coupon couponFromDb = await _db.Coupons.FirstOrDefaultAsync(c => c.CouponId == id);
                return couponFromDb;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }
        }

    }
}
