using Azure.Messaging.ServiceBus;
using Mango.Services.RewardsAPI.Message;
using Mango.Services.RewardsAPI.Messaging;
using Mango.Services.RewardsAPI.Services;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;

        private readonly string orderCreatedRewardSubscription;
        private readonly string orderCreatedTopic;

        private readonly IConfiguration _configuration;
        private readonly RewardService _rewardService;

        //we can create multiple instances of the Azure Service Bus processor
        private ServiceBusProcessor _rewardProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration, RewardService rewardService)
        {
            _configuration = configuration;
            _rewardService = rewardService;

            //we fetch the service bus conn string from appsetting.json
            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");

            //then we fetch the topic subscription name from appsetting for the OrderCreatedTopic
            orderCreatedTopic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");

            //fetch the subscription name from appsetting for the OrderCreatedRewardsUpdate_Subscription
            orderCreatedRewardSubscription = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedRewardsUpdate_Subscription");

            //we initialize the service bus client, we can use the same client for multiple processors
            var client = new ServiceBusClient(serviceBusConnectionString);

            //next we setup the client to listen to any message in the EmailShoppingCartQueue
            _rewardProcessor = client.CreateProcessor(orderCreatedTopic, orderCreatedRewardSubscription);
        }

        public async Task Start()
        {
            //for the update reward listener
            _rewardProcessor.ProcessMessageAsync += OnNewOrderRewardsRequestReceived;
            _rewardProcessor.ProcessErrorAsync += ErrorHandler;

            await _rewardProcessor.StartProcessingAsync();
        }
      
        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.Message);
            return Task.CompletedTask;
        }

        private async Task OnNewOrderRewardsRequestReceived(ProcessMessageEventArgs arg)
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
                await _rewardService.UpdateRewards(objMessage);
                await arg.CompleteMessageAsync(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task Stop()
        {
            //for the update reward listener
            await _rewardProcessor.StopProcessingAsync();
            await _rewardProcessor.DisposeAsync();
        }
    }
}
