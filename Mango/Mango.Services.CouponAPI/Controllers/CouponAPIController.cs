using AutoMapper;
using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.DTO;
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
        private ResponseDTO _response;
        private IMapper _mapper;

        public CouponAPIController(ApplicationDbContext dbContext, IMapper mapper)
        {
            _db = dbContext;
            _mapper = mapper;
            _response = new ResponseDTO();
        }
        [HttpGet]
        public ResponseDTO Get() {
            try
            {
                IEnumerable<Coupon> objListOfCoupons = _db.Coupons.ToList();

                //we use the response model as the output for the endpoint and
                //pass the data to the automapper to map the coupon model to the coupon dto
                _response.Result = _mapper.Map<IEnumerable<CouponDTO>>(objListOfCoupons);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        [HttpGet(/*"{id:int}"*/)]
        [Route("{id:int}")]
        public async Task<ResponseDTO> Get(int id)
        {
            try
            {
                Coupon couponFromDb = await _db.Coupons.FirstOrDefaultAsync(c => c.CouponId == id);
                _response.Result = _mapper.Map<CouponDTO>(couponFromDb);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

    }
}
