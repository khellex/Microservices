using static Mango.Web.Utilities.StaticDetails;

namespace Mango.Web.Models
{
    /// <summary>
    /// This is a request DTO based in the web project
    /// All the requested data from this DTO will have
    /// the following request layout
    /// </summary>
    public class RequestDto
    {
        public ApiType ApiType { get; set; } = ApiType.GET;
        public string Url { get; set; }
        public object Data { get; set; }
        public string AccessToken { get; set; }
    }
}
