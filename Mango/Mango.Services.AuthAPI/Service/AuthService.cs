using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Mango.Services.AuthAPI.Service
{
    public class AuthService : IAuthService
    {
        /// <summary>
        /// Inside the Auth Service implementation we inject
        /// the ApplicationDbContext, user manager and role manager to handle
        /// the identity usage for registering and user login
        /// </summary>
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AuthService> _logger;
        private readonly IJwtGenerator _jwtGenerator;

        public AuthService(ApplicationDbContext db, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager,
            ILogger<AuthService> logger, IJwtGenerator jwtGenerator)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
            _jwtGenerator = jwtGenerator;
        }
        /// <summary>
        /// This service checks if the username exists in our application
        /// and then validates the input password, if the user exists and
        /// the password is valid, we will generate the user details and the JWT
        /// </summary>
        /// <param name="loginRequestDto"></param>
        /// <returns name="loginResponseDto">User details and the JWT</returns>
        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            LoginResponseDto loginResponse = new();

            try
            {
                var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDto.Username.ToLower());
                bool validUserPassword = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

                //this means the user data is available and the password is verified
                if (user != null || validUserPassword)
                {
                    loginResponse.User = new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber
                    };
                    //JWT generator
                    loginResponse.Token = _jwtGenerator.GenerateToken(user);
                }
                else
                {
                    loginResponse.User = null;
                    loginResponse.Token = "";
                }
                return loginResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while user login");
            }
            return loginResponse;
        }
        /// <summary>
        /// Register service will utilize the user manager role manager identity
        /// classes and register the new user into the AspNewUsers table with the specified
        /// details
        /// </summary>
        /// <param name="registrationRequestDto"></param>
        /// <returns name="responseDto">message and flags based on the user registration status</returns>
        public async Task<ResponseDto> Register(RegistrationRequestDto registrationRequestDto)
        {
            ResponseDto responseDto = new();

            try
            {
                ApplicationUser user = new()
                {
                    Name = registrationRequestDto.Name,
                    Email = registrationRequestDto.Email,
                    UserName = registrationRequestDto.Email,
                    NormalizedEmail = registrationRequestDto.Email.ToUpper(),
                    PhoneNumber = registrationRequestDto.PhoneNumber
                };

                var registration = await _userManager.CreateAsync(user, registrationRequestDto.Password);
                if (registration.Succeeded)
                {
                    responseDto.IsSuccess = true;
                    responseDto.Message = "User Registered successfully";
                    responseDto.Result = new UserDto()
                    {
                        Name = user.Name,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber
                    };
                    return responseDto;
                }
                else
                {
                    responseDto.Message = registration.Errors.FirstOrDefault().Description;
                    responseDto.IsSuccess = false;
                    _logger.LogWarning("User registration failed for {Email}: {Errors}", registrationRequestDto.Email, responseDto.Result);
                    return responseDto;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering user {Email}", registrationRequestDto.Email);
                responseDto.IsSuccess = false;
                responseDto.Message = "An unexpected error occurred.";
            }
            return responseDto;
        }
    }
}
