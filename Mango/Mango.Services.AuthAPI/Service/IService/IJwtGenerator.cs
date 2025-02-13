using Mango.Services.AuthAPI.Models;
using System.Security.Claims;

namespace Mango.Services.AuthAPI.Service.IService
{
    public interface IJwtGenerator
    {
        string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles);
    }
}
