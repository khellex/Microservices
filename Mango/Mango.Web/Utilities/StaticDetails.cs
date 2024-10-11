namespace Mango.Web.Utilities
{
    public class StaticDetails
    {
        public static string ApibaseURL { get; set; }
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        } 
    }
}
