using Mango.Web.Service.IService;
using Mango.Web.Utilities;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;

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
        /// Used to clear token and refresh token, once the user logs out
        /// </summary>
        public void ClearToken()
        {
            var httpContext = _contextAccessor.HttpContext;
            if (httpContext != null)
            {
                //clear the token
                httpContext.Response.Cookies.Delete(StaticDetails.TokenCookie);

                //clear the refresh token
                httpContext.Response.Cookies.Delete(StaticDetails.RefreshTokenCookie);
            }
        }
        /// <summary>
        /// Fetches the current in-session token 
        /// </summary>
        /// <returns></returns>
        public string? GetToken()
        {
            string? token = null;

            bool? hasToken = _contextAccessor.HttpContext?.Request.Cookies.TryGetValue(StaticDetails.TokenCookie, out token);

            return hasToken is true ? token : null;
        }
        /// <summary>
        /// Fetches the refresh token from the cookies.
        /// </summary>
        public string? GetRefreshToken()
        {
            string? refreshToken = null;

            bool? hasRefreshToken = _contextAccessor.HttpContext?.Request.Cookies.TryGetValue(StaticDetails.RefreshTokenCookie, out refreshToken);

            return hasRefreshToken is true ? refreshToken : null;
        }
        /// <summary>
        /// sets the token to the cookie when the user logs in
        /// </summary>
        /// <param name="token"></param>
        public void SetToken(string token, string refreshToken)
        {
            var httpContext = _contextAccessor.HttpContext;
            if (httpContext != null)
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, // Prevents access via JavaScript
                    Secure = true,   // Ensures it is sent only over HTTPS
                    SameSite = SameSiteMode.Lax, // Mitigates CSRF
                    Expires = DateTimeOffset.UtcNow.AddMinutes(20) // Set an expiration
                };
                var refreshTokenOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict, // More secure for refresh token
                    Expires = DateTimeOffset.UtcNow.AddDays(1)
                };

                httpContext.Response.Cookies.Append(StaticDetails.TokenCookie, token, cookieOptions);
                httpContext.Response.Cookies.Append(StaticDetails.RefreshTokenCookie, refreshToken, refreshTokenOptions);
            }
        }
        //within the same httpContext request, GetToken returns null,
        //hence we need to pass the token as parameter for the initial login
        public DateTime? GetTokenExpiry(string? token = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                token = GetToken();

                if (string.IsNullOrEmpty(token))
                {
                    return null; // No token found
                }
            }
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
            {
                return null; // Invalid token
            }

            var expiryTimestamp = jwtToken.Claims.FirstOrDefault(c=>c.Type == JwtRegisteredClaimNames.Exp)?.Value;

            if (string.IsNullOrEmpty(expiryTimestamp) || !long.TryParse(expiryTimestamp, out var expUnixTime))
            {
                return null; // Expiration claim is missing or invalid
            }

            return DateTimeOffset.FromUnixTimeSeconds(expUnixTime).UtcDateTime; // Convert Unix timestamp to DateTime
        }

    }
}
