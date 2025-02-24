using Microsoft.AspNetCore.SignalR;

namespace Mango.Web.Utilities
{
    public class SignalRSessionHub : Hub
    {
        public async Task SendSessionExpiry(string userId, DateTime expiryTime)
        {
            await Clients.User(userId).SendAsync("SessionExpiryTime", expiryTime);
        }

        public async Task ForceLogout()
        {
            await Clients.Caller.SendAsync("Logout", "Your session has expired.");
        }

        //public async Task TokenRefreshed()
        //{
        //    await Clients.Caller.SendAsync("TokenRefreshed", "Token successfully refreshed.");
        //}
    }
}
