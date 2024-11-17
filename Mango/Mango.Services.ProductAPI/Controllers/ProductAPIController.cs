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
    //[Authorize]
    public class ProductAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private ResponseDto _response;
        private IMapper _mapper;
        private ILogger<ProductAPIController> _logger;
        private readonly IConfiguration _configuration;

        public ProductAPIController(ApplicationDbContext db, IMapper mapper, ILogger<ProductAPIController> logger, IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _response = new();
            _logger = logger;
            _configuration = configuration;
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
        [HttpGet]
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
        /// Create/Add new product
        /// </summary>
        /// <param name="productDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ResponseDto> Post(ProductDto productDto) //adding [FromBody] tells the endpoint that the parameters will be coming from the request body
        {                                                          //when sending multi part form data, we can remove the [FromBody] annotation
            try
            {
                Product product = _mapper.Map<Product>(productDto);
                await _db.Products.AddAsync(product);
                await _db.SaveChangesAsync();
                _response.Message = "Product created successfully";

                if (productDto.Image != null)
                {
                    //name of the file
                    string fileName = product.ProductId + Path.GetExtension(productDto.Image.FileName);

                    //location where the file should be saved
                    string filePath = _configuration.GetValue<string>("ProductImagePath") + fileName;

                    var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                    using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
                    {
                        productDto.Image.CopyTo(fileStream);
                    }

                    //fetching the URL so we can pass it to the image URL, this fetches https://localhost:7000
                    var baseURL = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}";
                    product.ImageUrl = baseURL + "/ProductImages/" + fileName;
                    product.ImageLocalPath = filePath;
                }
                else
                {
                    //if no image is uploaded
                    product.ImageUrl = "https://placehold.co/600x400";
                }
                _db.Products.Update(product);
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
        /// Endpoint to update the full product object
        /// </summary>
        /// <param name="productDto"></param>
        /// <returns>Updated product object</returns>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ResponseDto> Put(ProductDto productDto)
        {
            try
            {
                Product product = _mapper.Map<Product>(productDto);

                //if productDto.Image is not null, it means new image has been uploaded
                if (productDto.Image != null)
                {
                    //if new image has been uploaded, we need to remove the existing image first
                    if (!string.IsNullOrEmpty(productDto.ImageUrl))
                    {
                        var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), productDto.ImageLocalPath);
                        FileInfo file = new FileInfo(oldFilePathDirectory);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }
                    //once old image is deleted, we can insert the new one

                    //name of the file
                    string fileName = product.ProductId + Path.GetExtension(productDto.Image.FileName);

                    //location where the file should be saved
                    string filePath = _configuration.GetValue<string>("ProductImagePath") + fileName;

                    var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                    using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
                    {
                        productDto.Image.CopyTo(fileStream);
                    }

                    //fetching the URL so we can pass it to the image URL, this fetches https://localhost:7000
                    var baseURL = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}";
                    product.ImageUrl = baseURL + "/ProductImages/" + fileName;
                    product.ImageLocalPath = filePath;
                }
                
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
                    if ( !string.IsNullOrEmpty(productFromDb.ImageUrl))
                    {
                        var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), productFromDb.ImageLocalPath);
                        FileInfo file = new FileInfo(oldFilePathDirectory);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }
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
