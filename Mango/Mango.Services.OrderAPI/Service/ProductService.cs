using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Service.IService;
using Newtonsoft.Json;

namespace Mango.Services.OrderAPI.Service
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
                //this client name should match with the name provided in the program.cs,
                //based on that the system understands we are trying to communicate with
                //the ProductAPI
                var client = _httpClientFactory.CreateClient("Product");

                //gets the data from the product endpoint
                var response = await client.GetAsync($"/api/product");

                if (!response.IsSuccessStatusCode)
                {
                    // makes sure if the http request was successful or failed
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
