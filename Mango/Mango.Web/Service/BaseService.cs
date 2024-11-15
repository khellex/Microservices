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
                    //adding this implementation to handle the multipart
                    //form data content for the image file uploads
                    if (RequestDto.ContentType == ContentType.MultipartFormData)
                    {
                        //*/* means if the content type is multipart form data,
                        //we should accept any media type and sub type
                        message.Headers.Add("Accept", "*/*");
                    }
                    else
                    {
                        //this header specifies to the endpoint that we will accept application/json content-type
                        message.Headers.Add("Accept", "application/json");

                    }
                    //Sending the Bearer token to the API
                    if (withBearer)
                    {
                        var token = _tokenProvider.GetToken();
                        message.Headers.Add("Authorization", $"Bearer {token}");
                    }

                    //this is used to build the request uri
                    //(the endpoint that will be executed to fulfill the request)
                    message.RequestUri = new Uri(RequestDto.Url);

                    //handling the MultipartFormData for POST and PUT requests
                    if (RequestDto.ContentType == ContentType.MultipartFormData)
                    {
                        var content = new MultipartFormDataContent();

                        //in this code, we check for the content type properties inside the requestdto.data property
                        foreach (var prop in RequestDto.Data.GetType().GetProperties())
                        {
                            //here we check if the requestdto.data is a form file
                            var value = prop.GetValue(RequestDto.Data);

                            //here we only check if we encounter a form file in our model/dto
                            if (value is FormFile)
                            {
                                var file = (FormFile)value;

                                //then we check if the form file is null, if not then we need to add it to our request content
                                if (file != null)
                                {
                                    //here we add the content, we will create a file stream,
                                    //with the property name and the file name 
                                    content.Add(new StreamContent(file.OpenReadStream()), prop.Name, file.FileName);
                                }
                            }
                            //for all the other properties, which are not form file
                            else
                            {
                                //if value is null, we assign empty string, otherwise the value is passed
                                content.Add(new StringContent(value == null ? "" : value.ToString()), prop.Name);
                            }
                        }
                        message.Content = content;
                    }
                    else
                    {   //this is for the application/json content data for POST or the PUT request that we make
                        if (RequestDto.Data != null)
                        {
                            message.Content = new StringContent(JsonConvert.SerializeObject(RequestDto.Data), Encoding.UTF8, "application/json");
                        }
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
