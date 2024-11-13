using Mango.Services.RewardsAPI.Data;
using Mango.Services.RewardsAPI.Message;
using Mango.Services.RewardsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.RewardsAPI.Services
{
    public class RewardService : IRewardService
    {
        //singleton implementation of the AppDbContext
        private DbContextOptions<ApplicationDbContext> _dbOptions;

        //injecting the implementation in to the constructor
        public RewardService(DbContextOptions<ApplicationDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        //log the user rewards in table in the db
        public async Task<bool> UpdateRewards(RewardsMessage rewardsMessage)
        {
            try
            {
                Rewards rewards = new()
                {
                    RewardsActivity = rewardsMessage.RewardsActivity,
                    UserId = rewardsMessage.UserId,
                    OrderId = rewardsMessage.OrderId,
                    RewardsDate = DateTime.Now,
                };
                await using var _db = new ApplicationDbContext(_dbOptions);
                await _db.AddAsync(rewards);
                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                return false;
            }
        }
    }
}
