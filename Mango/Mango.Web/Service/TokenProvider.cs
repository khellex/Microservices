using Mango.Web.Service.IService;
using Mango.Web.Utilities;

namespace Mango.Web.Service
{
    public class TokenProvider : ITokenProvider
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public TokenProvider(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        /// <summary>
        /// Used to clear token, once the user logs out
        /// </summary>
        public void ClearToken()
        {
            _contextAccessor.HttpContext?.Response.Cookies.Delete(StaticDetails.TokenCookie);
        }
        /// <summary>
        /// Fetches the current in-session token 
        /// </summary>
        /// <returns></returns>
        public string? GetToken()
        {
            string? token = null;

            bool? hasToken = _contextAccessor.HttpContext?.Request.Cookies.TryGetValue(StaticDetails.TokenCookie,out token);

            return hasToken is true ? token : null;
        }
        /// <summary>
        /// sets the token to the cookie when the user logs in
        /// </summary>
        /// <param name="token"></param>
        public void SetToken(string token)
        {
            var httpContext = _contextAccessor.HttpContext;
            if (httpContext != null)
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, // Prevents access via JavaScript
                    Secure = true,   // Ensures it is sent only over HTTPS
                    SameSite = SameSiteMode.Strict, // Mitigates CSRF
                    Expires = DateTimeOffset.UtcNow.AddDays(1) // Set an expiration
                };

                httpContext.Response.Cookies.Append(StaticDetails.TokenCookie, token, cookieOptions);
            }
        }
    }
}
