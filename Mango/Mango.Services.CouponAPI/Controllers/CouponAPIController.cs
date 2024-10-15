using AutoMapper;
using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.CouponAPI.Controllers
{
    [Route("api/Coupon")]
    [ApiController]
    public class CouponAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private ResponseDTO _response;
        private IMapper _mapper;
        private ILogger<CouponAPIController> _logger;

        public CouponAPIController(ApplicationDbContext dbContext, IMapper mapper, ILogger<CouponAPIController> logger)
        {
            _db = dbContext;
            _mapper = mapper;
            _response = new ResponseDTO();
            _logger = logger;
        }
        /// <summary>
        /// GETs full list of coupons available in system
        /// </summary>
        /// <returns>
        /// Full coupon object
        /// </returns>
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
                _logger.LogError(ex, "An error occurred while processing the request.");
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        /// <summary>
        /// GETs list of coupons available in system based on the id passed
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Coupon object based on the supplied couponID</returns>
        [HttpGet()]
        [Route("{id:int}")]
        public async Task<ResponseDTO> Get(int id)
        {
            try
            {
                Coupon couponFromDb = await _db.Coupons.FirstOrDefaultAsync(c => c.CouponId == id);
                if (couponFromDb == null)
                {
                    _response.IsSuccess = false;
                }
                _response.Result = _mapper.Map<CouponDTO>(couponFromDb);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        /// <summary>
        /// GETs list of coupons available in system based on the coupon code passed
        /// </summary>
        /// <param name="code"></param>
        /// <returns>Coupon object based on the supplied coupon code</returns>
        [HttpGet("GetByCode/{code}")]
        public async Task<ResponseDTO> GetByCode(string code)
        {
            try
            {
                Coupon couponFromDb = await _db.Coupons.FirstOrDefaultAsync(c => c.CouponCode.ToLower() == code.ToLower());
                if (couponFromDb== null)
                {
                    _response.IsSuccess = false;
                }
                _response.Result = _mapper.Map<CouponDTO>(couponFromDb);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        /// <summary>
        /// Create/Add new coupon
        /// </summary>
        /// <param name="couponDTO"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDTO> Post([FromBody] CouponDTO couponDTO)
        {
            try
            {
                Coupon coupon = _mapper.Map<Coupon>(couponDTO);
                await _db.Coupons.AddAsync(coupon);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        /// <summary>
        /// Endpoint to update the full coupon object
        /// </summary>
        /// <param name="couponDTO"></param>
        /// <returns>Updated coupon object</returns>
        [HttpPut]
        public async Task<ResponseDTO> Put([FromBody] CouponDTO couponDTO)
        {
            try
            {
                Coupon coupon = _mapper.Map<Coupon>(couponDTO);
                _db.Coupons.Update(coupon);
                await _db.SaveChangesAsync();
                _response.Result = _mapper.Map<CouponDTO>(coupon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
        /// <summary>
        /// Endpoint to delete the selected coupon entry
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Removes the entry from db</returns>
        [HttpDelete("{id}")]
        public async Task<ResponseDTO> Delete(int id)
        {
            try
            {
                Coupon couponFromDb = await _db.Coupons.FirstOrDefaultAsync(c => c.CouponId==id);
                if (couponFromDb != null)
                {
                    _db.Coupons.Remove(couponFromDb);
                    await _db.SaveChangesAsync();
                    _response.Result = _mapper.Map<CouponDTO>(couponFromDb);
                }
                else
                {
                    _response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
    }
}
