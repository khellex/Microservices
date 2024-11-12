using Mango.Services.RewardsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.RewardsAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext( DbContextOptions<ApplicationDbContext> options ) : base(options)
        {
        }

        public DbSet<Rewards> Rewards { get; set; }
    }
}
