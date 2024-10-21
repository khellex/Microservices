namespace Mango.Web.Utilities
{
    public class StaticDetails
    {
        //the base URLs of each service is different,
        //hence we need to configure the base URLs in
        //the web project separately
        public static string? CouponApiBaseURL { get; set; }
        public static string? AuthApiBaseURL { get; set; }
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        } 
        //roles
        public const string AdminRole = "Admin";
        public const string CustomerRole = "Customer";

        //session cookie
        public const string TokenCookie = "JWT";
    }
}
