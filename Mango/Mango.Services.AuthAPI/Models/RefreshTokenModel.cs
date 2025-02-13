namespace Mango.Services.AuthAPI.Models
{
    public class RefreshTokenModel
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public DateTime TokenExpiry { get; set; }
        public bool IsRevoked { get; set; }
    }
}
