using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mango.Web.Controllers
{
    /// <summary>
    /// Controller to handle all the auth related
    /// requests for the web project. Everything from 
    /// the user login to registration
    /// </summary>
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IHubContext<SignalRSessionHub> _hubContext;

        public AuthController(IAuthService authService, ITokenProvider tokenProvider, IHttpContextAccessor contextAccessor, IHubContext<SignalRSessionHub> hubContext)
        {
            _authService = authService;
            _tokenProvider = tokenProvider; 
            _contextAccessor = contextAccessor;
            _hubContext = hubContext;
        }
        #region Log in
        /// <summary>
        /// Loads the view for the login page
        /// </summary>
        /// <returns>The login page with new loginRequestDto object</returns>
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            LoginRequestDto loginRequestDto = new();
            return View();
        }
        /// <summary>
        /// Post action for the user login, will validate
        /// the username and password passed in the
        /// loginRequestDto and allow the user to log in
        /// to the application
        /// </summary>
        /// <param name="loginRequestDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {
            ResponseDto? responseDto = await _authService.LoginAsync(loginRequestDto);
            if (responseDto != null && responseDto.IsSuccess)
            {
                LoginResponseDto? loginResponseDto = JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(responseDto.Result));

                //setting the sign-in session for the logged in user
                await SignInUser(loginResponseDto);

                //setting the session token cookie for the signed in user
                _tokenProvider.SetToken(loginResponseDto.Token, loginResponseDto.RefreshToken);

                //fetching the token expiry, for current httpcontext request, until the request is not completed
                // the token does not get set into cookie, hence we need to pass the token as parameter to GetTokenExpiry func
                var tokenExpiry = _tokenProvider.GetTokenExpiry(token: loginResponseDto.Token);

                await _hubContext.Clients.All.SendAsync("SessionExpiryTime", tokenExpiry);

                TempData["success"] = responseDto.Message;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["error"] = responseDto?.Message;
                return View(loginRequestDto);
            }
        }
        #endregion
        #region Registration
        /// <summary>
        /// Loads the user registration page with the role dropdown
        /// </summary>
        /// <returns>User registration page</returns>
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            //supplies the role text and value to the role dropdown during user registration
            List<SelectListItem> roleList = new()
            {
                new SelectListItem { Text= StaticDetails.AdminRole, Value= StaticDetails.AdminRole },
                new SelectListItem { Text= StaticDetails.CustomerRole, Value= StaticDetails.CustomerRole },
            };
            ViewBag.RoleList = roleList;
            return View();
        }
        /// <summary>
        /// Accepts user registration details and registers
        /// the user in the application, along with the user
        /// role, if no role is set then the default role is
        /// set to Customer
        /// </summary>
        /// <param name="registrationRequestDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDto registrationRequestDto)
        {
            ResponseDto? result = await _authService.RegisterAsync(registrationRequestDto);
            if (result != null && result.IsSuccess)
            {
                //after successful user registration,
                //if role is empty, then by default
                //"Customer" role is set
                if (string.IsNullOrEmpty(registrationRequestDto.Role))
                {
                    registrationRequestDto.Role = StaticDetails.CustomerRole;
                }
                ResponseDto? assignRole = await _authService.AssignRoleAsync(registrationRequestDto);
                if (assignRole != null && assignRole.IsSuccess)
                {
                    TempData["success"] = result?.Message;
                    return RedirectToAction(nameof(Login));
                }
            }
            else
            {
                TempData["error"] = result?.Message;
            }
            //the registration was unsuccessful, then the registration page is reloaded with the following details
            List<SelectListItem> roleList = new()
            {
                new SelectListItem { Text= StaticDetails.AdminRole, Value= StaticDetails.AdminRole },
                new SelectListItem { Text= StaticDetails.CustomerRole, Value= StaticDetails.CustomerRole },
            };
            ViewBag.RoleList = roleList;
            return View(registrationRequestDto);
        }
        #endregion
        #region Log out
        /// <summary>
        /// Logs out the user and clears their session token cookie
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            string? token = Request.Cookies["refreshToken"];

            //signs out the user
            await _contextAccessor.HttpContext.SignOutAsync();

            //clears the token session cookie
            _tokenProvider.ClearToken();

            if (!string.IsNullOrEmpty(token))
            {
                await _authService.LogoutAsync(new RefreshTokenRequestDto { RefreshToken = token });
            }

            TempData["success"] = "Successfully logged out";
            return RedirectToAction("Index","Home");
        }
        #endregion
        #region Sign in 
        private async Task SignInUser(LoginResponseDto loginResponseDto)
        {
            var handler = new JwtSecurityTokenHandler();

            var jwt = handler.ReadJwtToken(loginResponseDto.Token);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email, jwt.Claims.FirstOrDefault(j => j.Type == JwtRegisteredClaimNames.Email).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Iss, jwt.Claims.FirstOrDefault(j => j.Type == JwtRegisteredClaimNames.Iss).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, jwt.Claims.FirstOrDefault(j => j.Type == JwtRegisteredClaimNames.Sub).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name, jwt.Claims.FirstOrDefault(j => j.Type == JwtRegisteredClaimNames.Name).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Jti, jwt.Claims.FirstOrDefault(j => j.Type == JwtRegisteredClaimNames.Jti).Value));

            //this is the built-in identity claims which will utilise the the logged in users name and role
            identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(j => j.Type == JwtRegisteredClaimNames.Email).Value));
            identity.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(j => j.Type == "role").Value));

            var principal = new ClaimsPrincipal(identity);

            await _contextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
        #endregion
    }
}
