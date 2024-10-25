using AutoMapper;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ProductAPI.Controllers
{
    [Route("api/product")]
    [ApiController]
    [Authorize]
    public class ProductAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private ResponseDto _response;
        private IMapper _mapper;
        private ILogger<ProductAPIController> _logger;

        public ProductAPIController(ApplicationDbContext db, IMapper mapper, ILogger<ProductAPIController> logger)
        {
            _db = db;
            _mapper = mapper;
            _response = new();
            _logger = logger;
        }
        /// <summary>
        /// GETs full list of products available in system
        /// </summary>
        /// <returns>
        /// Full product object
        /// </returns>
        [HttpGet]
        public ResponseDto Get()
        {
            try
            {
                IEnumerable<Product> objListOfProducts = _db.Products.ToList();

                //we use the response model as the output for the endpoint and
                //pass the data to the automapper to map the product model to the product dto
                _response.Result = _mapper.Map<IEnumerable<ProductDto>>(objListOfProducts);
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
        /// GETs list of products available in system based on the id passed
        /// </summary>
        /// <param name="productId"></param>
        /// <returns>Product object based on the supplied productID</returns>
        [HttpGet()]
        [Route("{productId:int}")]
        public async Task<ResponseDto> Get(int productId)
        {
            try
            {
                Product? productFromDb = await _db.Products.FirstOrDefaultAsync(x => x.ProductId == productId);
                if (productFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Something went wrong";
                }
                _response.Result = _mapper.Map<ProductDto>(productFromDb);
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
        //[HttpGet("GetByCode/{code}")]
        //public async Task<ResponseDto> GetByCode(string code)
        //{
        //    try
        //    {
        //        Coupon couponFromDb = await _db.Coupons.FirstOrDefaultAsync(c => c.CouponCode.ToLower() == code.ToLower());
        //        if (couponFromDb == null)
        //        {
        //            _response.IsSuccess = false;
        //        }
        //        _response.Result = _mapper.Map<CouponDto>(couponFromDb);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An error occurred while processing the request.");
        //        _response.IsSuccess = false;
        //        _response.Message = ex.Message;
        //    }
        //    return _response;
        //}
        /// <summary>
        /// Create/Add new product
        /// </summary>
        /// <param name="productDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ResponseDto> Post([FromBody] ProductDto productDto)
        {
            try
            {
                Product product = _mapper.Map<Product>(productDto);
                await _db.Products.AddAsync(product);
                await _db.SaveChangesAsync();
                _response.Message = "Product created successfully";
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
        /// Endpoint to update the full product object
        /// </summary>
        /// <param name="productDto"></param>
        /// <returns>Updated product object</returns>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ResponseDto> Put([FromBody] ProductDto productDto)
        {
            try
            {
                Product product = _mapper.Map<Product>(productDto);
                _db.Products.Update(product);
                await _db.SaveChangesAsync();
                _response.Result = _mapper.Map<ProductDto>(product);
                _response.Message = "Product updated successfully.";
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
        /// Endpoint to delete the selected product entry
        /// </summary>
        /// <param name="productId"></param>
        /// <returns>Removes the entry from db</returns>
        [HttpDelete("{productId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ResponseDto> Delete(int productId)
        {
            try
            {
                Product? productFromDb = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
                if (productFromDb != null)
                {
                    _db.Products.Remove(productFromDb);
                    await _db.SaveChangesAsync();
                    _response.Result = _mapper.Map<ProductDto>(productFromDb);
                    _response.Message = "Product deleted successfully.";
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
