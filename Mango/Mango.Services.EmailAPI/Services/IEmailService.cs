using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models.Dto;

namespace Mango.Services.EmailAPI.Services
{
    public interface IEmailService
    {
        Task EmailCartAndLog(CartDto cartDto);
        Task EmailUserRegistrationAndLog(string email);
        Task LogPlacedOrder(RewardsMessage rewardsMessage);
    }
}
