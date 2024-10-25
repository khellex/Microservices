using Mango.Web.Models;

namespace Mango.Web.Service.IService
{
    public interface IProductService
    {
        //public Task<ResponseDto?> GetProductByCodeAsync(string code);
        public Task<ResponseDto?> GetProductByIdAsync(int id);
        public Task<ResponseDto?> GetAllProductsAsync();
        public Task<ResponseDto?> CreateProductsAsync(ProductDto productDto);
        public Task<ResponseDto?> UpdateProductsAsync(ProductDto productDto);
        public Task<ResponseDto?> DeleteProductsAsync(int id);
    }
}
