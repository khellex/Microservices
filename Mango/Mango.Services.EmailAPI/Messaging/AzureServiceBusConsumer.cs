using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;

        private readonly string emailCartQueue;
        private readonly string userRegistrationQueue;

        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        //we can create multiple instances of the Azure Service Bus processor
        private ServiceBusProcessor _emailCartProcessor;
        private ServiceBusProcessor _userRegistrationProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;

            //we fetch the service bus conn string from appsetting.json
            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");

            //then we fetch the queue name from appsetting for the EmailShoppingCartQueue
            emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");

            //fetch the queue name from appsetting for the UserRegistrationQueue
            userRegistrationQueue = _configuration.GetValue<string>("TopicAndQueueNames:UserRegistrationQueue");

            //we initialize the service bus client, we can use the same client for multiple processors
            var client = new ServiceBusClient(serviceBusConnectionString);

            //next we setup the client to listen to any message in the EmailShoppingCartQueue
            _emailCartProcessor = client.CreateProcessor(emailCartQueue);

            //next we setup the client to listen to any message in the UserRegistrationQueue
            _userRegistrationProcessor = client.CreateProcessor(userRegistrationQueue);
        }

        public async Task Start()
        {
            //for the cart email
            _emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestRecieved;
            _emailCartProcessor.ProcessErrorAsync += ErrorHandler;

            await _emailCartProcessor.StartProcessingAsync();

            //for user registration email
            _userRegistrationProcessor.ProcessMessageAsync += OnUserRegistrationSuccessful;
            _userRegistrationProcessor.ProcessErrorAsync += ErrorHandler;

            await _userRegistrationProcessor.StartProcessingAsync();
        }

        private async Task OnUserRegistrationSuccessful(ProcessMessageEventArgs args)
        {
            //this is where we will receive the message from the service bus
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            string email = JsonConvert.DeserializeObject<string>(body);

            try
            {
                //this function will send the message to the Db to log it
                //( its supposed to mimic the email functionality, but we have
                //not implemented it since it requires sendgrid ) 
                await _emailService.EmailUserRegistrationAndLog(email);
                await args.CompleteMessageAsync(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.Message);
            return Task.CompletedTask;
        }

        private async Task OnEmailCartRequestRecieved(ProcessMessageEventArgs arg)
        {
            //this is where we will receive the message from the service bus
            var message = arg.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            CartDto objMessage = JsonConvert.DeserializeObject<CartDto>(body);

            try
            {
                //this function will send the message to the Db to log it
                //( its supposed to mimic the email functionality, but we have
                //not implemented it since it requires sendgrid ) 
                await _emailService.EmailCartAndLog(objMessage);
                await arg.CompleteMessageAsync(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task Stop()
        {
            //for the email cart listener
            await _emailCartProcessor.StopProcessingAsync();
            await _emailCartProcessor.DisposeAsync();

            //for the user registration listener
            await _userRegistrationProcessor.StopProcessingAsync();
            await _userRegistrationProcessor.DisposeAsync();

        }
    }
}
