using Mango.Web.Models;
using Mango.Web.Service.IService;
using static Mango.Web.Utilities.StaticDetails;

namespace Mango.Web.Service
{
    public class ProductService : IProductService
    {
        //injecting the IBaseService interface
        private readonly IBaseService _baseService;

        public ProductService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> CreateProductsAsync(ProductDto productDto)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.POST,
                Data = productDto,
                Url = ProductApiBaseURL + "/api/product",
                ContentType = ContentType.MultipartFormData, //we need to explicitly mention MultipartFormData wherever we are passing that content from
            }));
        }

        public async Task<ResponseDto?> DeleteProductsAsync(int id)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.DELETE,
                Url = ProductApiBaseURL + "/api/product/" + id
            }));
        }

        public async Task<ResponseDto?> GetAllProductsAsync()
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.GET,
                Url = ProductApiBaseURL + "/api/product"
            }));
        }

        public async Task<ResponseDto?> GetProductByIdAsync(int id)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.GET,
                Url = ProductApiBaseURL + "/api/product/" + id
            }));
        }

        public async Task<ResponseDto?> UpdateProductsAsync(ProductDto productDto)
        {
            return (await _baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiType.PUT,
                Data = productDto,
                Url = ProductApiBaseURL + "/api/product",
                ContentType = ContentType.MultipartFormData, //we need to explicitly mention MultipartFormData wherever we are passing that content from
            }));
        }
    }
}
