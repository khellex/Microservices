using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Message;
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

        //this is for the reward topic 
        private readonly string orderCreatedEmailSubscription;
        private readonly string orderCreatedTopic;

        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        //we can create multiple instances of the Azure Service Bus processor
        private ServiceBusProcessor _emailCartProcessor;
        private ServiceBusProcessor _userRegistrationProcessor;
        private ServiceBusProcessor _orderCreatedEmailProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;

            //we fetch the service bus conn string from appsetting.json
            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");

            #region Service Bus Queues for Shopping Cart and User registration

            //then we fetch the queue name from appsetting for the EmailShoppingCartQueue
            emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");

            //fetch the queue name from appsetting for the UserRegistrationQueue
            userRegistrationQueue = _configuration.GetValue<string>("TopicAndQueueNames:UserRegistrationQueue");

            #endregion

            #region Service Bus Topics for Order and Rewards
            //then we fetch the topic subscription name from appsetting for the OrderCreatedTopic
            orderCreatedTopic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");

            //fetch the subscription name from appsetting for the OrderCreatedRewardsUpdate_Subscription
            orderCreatedEmailSubscription = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedEmail_Subscription");
            #endregion

            //we initialize the service bus client, we can use the same client for multiple processors
            var client = new ServiceBusClient(serviceBusConnectionString);

            #region Processors for queues and topics
            //next we setup the client to listen to any message in the EmailShoppingCartQueue
            _emailCartProcessor = client.CreateProcessor(emailCartQueue);

            //next we setup the client to listen to any message in the UserRegistrationQueue
            _userRegistrationProcessor = client.CreateProcessor(userRegistrationQueue);

            //processor to listen to the order created email Topic
            _orderCreatedEmailProcessor = client.CreateProcessor(orderCreatedTopic, orderCreatedEmailSubscription);
            #endregion
        }

        #region Generic helper methods
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

            //for the update reward listener
            _orderCreatedEmailProcessor.ProcessMessageAsync += OnNewOrderEmailRequestReceived;
            _orderCreatedEmailProcessor.ProcessErrorAsync += ErrorHandler;

            await _orderCreatedEmailProcessor.StartProcessingAsync();
        }
        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.Message);
            return Task.CompletedTask;
        }

        public async Task Stop()
        {
            //for the email cart listener
            await _emailCartProcessor.StopProcessingAsync();
            await _emailCartProcessor.DisposeAsync();

            //for the user registration listener
            await _userRegistrationProcessor.StopProcessingAsync();
            await _userRegistrationProcessor.DisposeAsync();

            //for the order email listener
            await _orderCreatedEmailProcessor.StopProcessingAsync();
            await _orderCreatedEmailProcessor.DisposeAsync();
        }
        #endregion

        #region User Registration email
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
        #endregion

        #region Cart Email
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
        #endregion
        private async Task OnNewOrderEmailRequestReceived(ProcessMessageEventArgs arg)
        {
            //this is where we will receive the message from the service bus
            var message = arg.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            RewardsMessage objMessage = JsonConvert.DeserializeObject<RewardsMessage>(body);

            try
            {
                //this function will send the message to the Db to log it
                //( its supposed to mimic the email functionality, but we have
                //not implemented it since it requires sendgrid ) 
                await _emailService.LogPlacedOrder(objMessage);
                await arg.CompleteMessageAsync(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
