using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Mango.Services.EmailAPI.Services
{
    public class EmailService : IEmailService
    {
        //singleton implementation of the AppDbContext
        private DbContextOptions<ApplicationDbContext> _dbOptions;

        //injecting the implementation in to the constructor
        public EmailService(DbContextOptions<ApplicationDbContext> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        //we cannot inject the appDbContext here since it is
        //a scoped implementation, and the Email Service is a singleton.
        //To make sure we don't run into any captive dependencies,
        //we need to implement a singleton implementation of the the appDbContext
        public async Task EmailCartAndLog(CartDto cartDto)
        {
            StringBuilder message = new();

            message.AppendLine("<br/> Email Cart requested");
            message.AppendLine("<br/> Cart Total:" + cartDto.CartHeaderDto.CartTotal);
            message.Append("<br/>");
            message.Append("<ul>");
            foreach (var item in cartDto.CartDetailsDto)
            {
                message.Append("<li>");
                message.Append(item.ProductDto.Name + "x" + item.Count);
                message.Append("</li>");
            }
            message.Append("</ul>");

            //this function will log the email attempt
            await EmailAndLog(message.ToString(), cartDto.CartHeaderDto.Email);
        }
        //this function is used to log the user registration email 
        public async Task EmailUserRegistrationAndLog(string email)
        {
            string genericEmail = "admin@gmail.com";

            string messageBody = "New User Registered :" + email;

            await EmailAndLog(messageBody, genericEmail);
        }

        public async Task LogPlacedOrder(RewardsMessage rewardsMessage)
        {
            string message = "New Order placed. </br> Order ID: " + rewardsMessage.OrderId;
            await EmailAndLog(message, "carp@gmail.com");
        }

        //generic function which will collect the info
        //and log the EmailLoggers table in the db
        private async Task<bool> EmailAndLog(string message, string email)
        {
            try
            {
                EmailLogger emailLogger = new()
                {
                    Email = email,
                    Message = message,
                    EmailSent = DateTime.Now,
                };
                await using var _db = new ApplicationDbContext(_dbOptions);
                await _db.AddAsync(emailLogger);
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
