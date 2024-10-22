using Mango.Web.Models;
using Mango.Web.Service.IService;
using Newtonsoft.Json;
using System.Text;
using static Mango.Web.Utilities.StaticDetails;

namespace Mango.Web.Service
{
    public class BaseService : IBaseService
    {
        /// <summary>
        /// The IHttpClientFactory interface in .NET is a part of the System.Net.Http
        /// namespace and is used for sending HTTP requests and receiving HTTP
        /// responses from a resource, such as a web API.
        /// </summary>
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenProvider _tokenProvider;

        public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider)
        {
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
        }
        public async Task<ResponseDto?> SendAsync(RequestDto RequestDto, bool withBearer = true)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient("MangoAPI");

                //reusing HttpRequestMessage class and properly disposing it once done
                using (HttpRequestMessage message = new HttpRequestMessage())
                {
                    //this header specifies to the endpoint that we will accept application/json content-type
                    message.Headers.Add("Accept", "application/json");

                    //Sending the Bearer token to the API
                    if (withBearer)
                    {
                        var token = _tokenProvider.GetToken();
                        message.Headers.Add("Authorization", $"Bearer {token}");
                    }

                    //this is used to build the request uri
                    //(the endpoint that will be executed to fulfill the request)
                    message.RequestUri = new Uri(RequestDto.Url);

                    //this is for the content data for POST or the PUT request that we make
                    if (RequestDto.Data != null)
                    {
                        message.Content = new StringContent(JsonConvert.SerializeObject(RequestDto.Data), Encoding.UTF8, "application/json");
                    }

                    //we receive the response from the ResponseDto that we have set up
                    HttpResponseMessage? apiResponse = null;

                    //based on the endpoint type, we will apply the request method type
                    switch (RequestDto.ApiType)
                    {
                        case ApiType.POST:
                            message.Method = HttpMethod.Post;
                            break;
                        case ApiType.PUT:
                            message.Method = HttpMethod.Put;
                            break;
                        case ApiType.DELETE:
                            message.Method = HttpMethod.Delete;
                            break;
                        default:
                            message.Method = HttpMethod.Get;
                            break;
                    }

                    //sending the message to the endpoint
                    apiResponse = await client.SendAsync(message);

                    switch (apiResponse.StatusCode)
                    {
                        case System.Net.HttpStatusCode.NotFound:
                            return new() { IsSuccess = false, Message = "Not Found" };

                        case System.Net.HttpStatusCode.Unauthorized:
                            return new() { IsSuccess = false, Message = "Unauthorized" };

                        case System.Net.HttpStatusCode.Forbidden:
                            return new() { IsSuccess = false, Message = "Access Denied" };

                        case System.Net.HttpStatusCode.InternalServerError:
                            return new() { IsSuccess = false, Message = "Internal Server Error" };

                        default:
                            var apiContent = await apiResponse.Content.ReadAsStringAsync();
                            var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
                            return apiResponseDto;
                    }
                }
            }
            catch (Exception ex)
            {
                var apiResponse = new ResponseDto()
                {
                    IsSuccess = false,
                    Message = ex.Message.ToString(),
                };
                return apiResponse;
            }
        }
    }
}
