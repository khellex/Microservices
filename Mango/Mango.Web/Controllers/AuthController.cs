using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public AuthController(IAuthService authService, ITokenProvider tokenProvider, IHttpContextAccessor contextAccessor)
        {
            _authService = authService;
            _tokenProvider = tokenProvider; 
            _contextAccessor = contextAccessor;
        }
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

                await SignInUser(loginResponseDto);

                _tokenProvider.SetToken(loginResponseDto.Token);
                TempData["success"] = "User logged in successfully.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("CustomError", responseDto.Message);
                return View(loginRequestDto);
            }
        }
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
                    TempData["success"] = "User registered successfully.";
                    return RedirectToAction(nameof(Login));
                }
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
        public async Task<IActionResult> Logout()
        {
            await _contextAccessor.HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();
            return RedirectToAction("Index","Home");
        }
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
            identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(j => j.Type == JwtRegisteredClaimNames.Email).Value));

            var principal = new ClaimsPrincipal(identity);
            await _contextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}
