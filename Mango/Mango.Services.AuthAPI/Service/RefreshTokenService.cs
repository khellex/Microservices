using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Mango.Services.AuthAPI.Service
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IDatabase _cache;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public RefreshTokenService(IConnectionMultiplexer redis, IJwtGenerator jwt, UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _cache = redis.GetDatabase(); //for accessing the redis cache db
            _jwtGenerator = jwt;
            _userManager = userManager;
            _config = config;
        }

        /// <summary>
        /// Logic to generate the refresh token. We use the RandomNumberGenerator
        /// class to generate a cryptographic random number for this purpose.
        /// </summary>
        /// <returns>a cryptographic random number</returns>
        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public async Task SaveRefreshTokenAsync(string userId, string token)
        {
            var refreshToken = new RefreshTokenModel
            {
                Token = token,
                UserId = userId,
                TokenExpiry = DateTime.UtcNow.AddDays(1), //refresh token expires after 1 day
                IsRevoked = false
            };

            string json = JsonSerializer.Serialize(refreshToken);
            await _cache.StringSetAsync($"refresh_token:{token}", json, TimeSpan.FromDays(1)); //here the TimeSpan.FromDays(1) is how long the refresh token will be held in cache
            await _cache.StringSetAsync($"refresh_token:{userId}", token, TimeSpan.FromDays(1));
        }

        public async Task<RefreshTokenModel?> ValidateRefreshTokenAsync(string token)
        {
            string? json = await _cache.StringGetAsync($"refresh_token:{token}");

            if (string.IsNullOrEmpty(json))
                return null;

            var refreshToken = JsonSerializer.Deserialize<RefreshTokenModel>(json);

            if (refreshToken == null || refreshToken.IsRevoked || refreshToken.TokenExpiry < DateTime.UtcNow)
                return null;

            // Ensure this is the user's latest refresh token
            string? latestToken = await _cache.StringGetAsync($"refresh_token:{refreshToken.UserId}");
            if (latestToken != token)
                return null; // Token is no longer valid

            return refreshToken;
        }

        public async Task<RefreshTokenResponseDto?> RefreshAccessTokenAsync(string refreshToken)
        {
            var tokenData = await ValidateRefreshTokenAsync(refreshToken);
            if (tokenData == null) return null;

            // Revoke old token by deleting it from cache
            await _cache.KeyDeleteAsync($"refresh_token:{refreshToken}");

            var user = await _userManager.FindByIdAsync(tokenData.UserId);
            var userRoles = await _userManager.GetRolesAsync(user);

            if (user == null) return null;

            // Generate new tokens
            var newRefreshToken = GenerateRefreshToken();
            var newAccessToken = _jwtGenerator.GenerateToken(user, userRoles);

            await SaveRefreshTokenAsync(tokenData.UserId, newRefreshToken);

            return new RefreshTokenResponseDto
            {
                RefreshToken = newRefreshToken,
                Token = newAccessToken,
                TokenExpiryTime = DateTime.UtcNow.AddMinutes(_config.GetValue<int>("JwtOptions:ExpiryMinutes")),
            };
        }
    }
}
