namespace Mango.Web.Service.IService
{
    public interface ITokenProvider
    {
        void SetToken(string token, string refreshToken);
        string? GetToken();
        string? GetRefreshToken();
        void ClearToken();
        DateTime? GetTokenExpiry(string? token = null);
    }
}
