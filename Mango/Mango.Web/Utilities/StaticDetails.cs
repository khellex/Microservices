namespace Mango.Web.Utilities
{
    public class StaticDetails
    {
        //the base URLs of each service is different,
        //hence we need to configure the base URLs in
        //the web project separately
        public static string? CouponApiBaseURL { get; set; }
        public static string? AuthApiBaseURL { get; set; }
        public static string? ProductApiBaseURL { get; set; }
        public static string? CartApiBaseURL { get; set; }
        public static string? OrderApiBaseURL { get; set; }
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        } 
        public enum ContentType
        {
            Json,
            MultipartFormData
        } 

        //roles
        public const string AdminRole = "Admin";
        public const string CustomerRole = "Customer";

        //session cookie
        public const string TokenCookie = "JWT";
        public const string RefreshTokenCookie = "refreshToken";

        public enum OrderStatus
        {
            Pending,
            Approved,
            ReadyForPickup,
            Completed,
            Refunded,
            Cancelled
        }
        public static readonly Dictionary<OrderStatus, string> Statuses = new()
        {
            { OrderStatus.Pending, "Pending" },
            { OrderStatus.Approved, "Approved" },
            { OrderStatus.ReadyForPickup, "Ready For Pickup" },
            { OrderStatus.Completed, "Completed" },
            { OrderStatus.Refunded, "Refunded" },
            { OrderStatus.Cancelled, "Cancelled" }
        };
    }
}
