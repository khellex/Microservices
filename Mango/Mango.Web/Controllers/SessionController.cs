using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class SessionController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        public SessionController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }
        #region SessionRefresh
        public async Task<IActionResult> RefreshSession()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                ResponseDto? responseDto = await _authService.RefreshTokenAsync(new RefreshTokenRequestDto() { RefreshToken = refreshToken });
                if (responseDto != null)
                {
                    LoginResponseDto? loginResponseDto = JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(responseDto.Result));
                    _tokenProvider.SetToken(loginResponseDto.Token, loginResponseDto.RefreshToken);

                    // Return success response with new expiry time
                    return Json(new { success = true, expiresAt = _tokenProvider.GetTokenExpiry(loginResponseDto.Token) });
                }
            }
            // Return error response if refresh fails
            return Json(new { success = false, message = "Session refresh failed" });
        }
    }
    #endregion
}

