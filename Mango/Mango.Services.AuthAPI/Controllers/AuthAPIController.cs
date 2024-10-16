using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        private readonly IAuthService _authService;
        protected ResponseDto _response;

        public AuthAPIController(IAuthService authService)
        {
            _authService = authService;
            _response = new();
        }
        /// <summary>
        /// Endpoint to register new user on the application
        /// </summary>
        /// <param name="registrationRequestDto"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto registrationRequestDto)
        {
            var response = await _authService.Register(registrationRequestDto);
            if (!response.IsSuccess)
            {
                _response.IsSuccess = response.IsSuccess;
                _response.Message = response.Message;
                return BadRequest(_response);
            }
            else
            {
                return Ok(_response);
            }
        }
        /// <summary>
        /// Endpoint for registered user to login to 
        /// the application using username and password.
        /// </summary>
        /// <param name="loginRequestDto"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            var loginResponse = await _authService.Login(loginRequestDto);
            if (loginResponse.User != null)
            {
                _response.Message = "Logged in successfully.";
                _response.Result = loginResponse;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.Message = "Username or password is incorrect.";
            return BadRequest(_response);
        }
    }
}
