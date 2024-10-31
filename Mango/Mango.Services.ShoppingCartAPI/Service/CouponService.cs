using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Newtonsoft.Json;
using System;

namespace Mango.Services.ShoppingCartAPI.Service
{
    public class CouponService : ICouponService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CouponService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<CouponDto> GetCouponAsync(string couponCode)
        {
            try
            {
                //this client name should match with the name provided in the program.cs,
                //based on that the system understands we are trying to communicate with
                //the ProductAPI
                var client = _httpClientFactory.CreateClient("Coupon");

                //gets the data from the product endpoint
                var response = await client.GetAsync($"/api/coupon/GetByCode/{couponCode}");

                if (!response.IsSuccessStatusCode)
                {
                    // makes sure if the http request was successful or failed
                    return new();
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ResponseDto>(content);

                if (apiResponse?.IsSuccess == true && apiResponse.Result != null)
                {
                    return JsonConvert.DeserializeObject<CouponDto>(apiResponse.Result.ToString())
                           ?? new();
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging and monitoring.
                Console.WriteLine($"Error fetching coupons: {ex.Message}");
            }
            return new();
        }
    }
}
