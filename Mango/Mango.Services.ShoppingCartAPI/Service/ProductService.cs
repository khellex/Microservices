using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Newtonsoft.Json;
using System;

namespace Mango.Services.ShoppingCartAPI.Service
{
    public class ProductService : IProductService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<IEnumerable<ProductDto>> GetProductAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Product");
                var response = await client.GetAsync($"/api/product");

                if (!response.IsSuccessStatusCode)
                {
                    // Log or handle unsuccessful responses if needed.
                    return new List<ProductDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ResponseDto>(content);

                if (apiResponse?.IsSuccess == true && apiResponse.Result != null)
                {
                    return JsonConvert.DeserializeObject<IEnumerable<ProductDto>>(apiResponse.Result.ToString())
                           ?? new List<ProductDto>();
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging and monitoring.
                Console.WriteLine($"Error fetching products: {ex.Message}");
            }
            return new List<ProductDto>();
        }
    }
}
