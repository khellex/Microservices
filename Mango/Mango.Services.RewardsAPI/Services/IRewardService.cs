using Mango.Services.RewardsAPI.Message;

namespace Mango.Services.RewardsAPI.Services
{
    public interface IRewardService
    {
        Task<bool> UpdateRewards(RewardsMessage rewardsMessage);
    }
}
