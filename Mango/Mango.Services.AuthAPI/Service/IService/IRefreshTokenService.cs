using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using System.Security.Claims;

namespace Mango.Services.AuthAPI.Service.IService
{
    public interface IRefreshTokenService
    {
        string GenerateRefreshToken();
        Task SaveRefreshTokenAsync(string userId, string token);
        Task<RefreshTokenResponseDto> RefreshAccessTokenAsync(string refreshToken);
        Task<RefreshTokenModel?> ValidateRefreshTokenAsync(string token);
        Task<bool> RevokeToken(string token);
    }
}
