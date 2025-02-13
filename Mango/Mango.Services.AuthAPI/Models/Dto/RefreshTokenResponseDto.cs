namespace Mango.Services.AuthAPI.Models.Dto
{
    public class RefreshTokenResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime TokenExpiryTime { get; set; }
    }
}