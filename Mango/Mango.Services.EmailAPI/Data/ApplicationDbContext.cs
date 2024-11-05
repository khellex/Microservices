using Mango.Services.EmailAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.EmailAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext( DbContextOptions<ApplicationDbContext> options ) : base(options)
        {
        }
        public DbSet<EmailLogger> EmailLogs { get; set; }
    }
}
