using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models.Dto;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;
        private readonly string emailCartQueue;
        private readonly IConfiguration _configuration;

        private ServiceBusProcessor _emailCartProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration)
        {
            _configuration = configuration;

            //we fetch the service bus conn string from appsetting.json
            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            //then we fetch the queue name from appsetting
            emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");

            //we initialize the service bus client
            var client = new ServiceBusClient(serviceBusConnectionString);

            //next we setup the client to listen to any message in the queue
            _emailCartProcessor = client.CreateProcessor(emailCartQueue);

            //handler to process the message received from the queue
        }

        public async Task Start()
        {
            _emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestRecieved;
            _emailCartProcessor.ProcessErrorAsync += ErrorHandler;

            await _emailCartProcessor.StartProcessingAsync();
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
                //TODO - log the email here
                await arg.CompleteMessageAsync(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task Stop()
        {
            await _emailCartProcessor.StopProcessingAsync();
            await _emailCartProcessor.DisposeAsync();
        }
    }
}
