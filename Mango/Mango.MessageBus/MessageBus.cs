using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.MessageBus
{
    /// <summary>
    /// This MessageBus implementation is used to send the message from
    /// the API project ( shopping cart API specifically ) to the
    /// Azure Service bus queue.
    /// The message queue will be receive at the Email Sender processor,
    /// where once the message is received the queue will be emptied one by one.
    /// </summary>
    public class MessageBus : IMessageBus
    {
        private readonly string ConnectionString = "";
        public async Task PublishMessage(object message, string topicOrQueueName)
        {
            // Define a simple retry policy: retry up to 3 times with a delay of 2 seconds between retries.
            AsyncRetryPolicy retryPolicy = Policy
                .Handle<Exception>() // You can customize the exception types to handle specific transient errors.
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        // Log or handle each retry attempt if needed
                        Console.WriteLine($"Retry {retryCount} encountered an error: {exception.Message}. Waiting {timeSpan} before retrying.");
                    });

            await retryPolicy.ExecuteAsync(async () =>
            {
                await using var client = new ServiceBusClient(ConnectionString);

                ServiceBusSender sender = client.CreateSender(topicOrQueueName);

                var jsonMessage = JsonConvert.SerializeObject(message);

                ServiceBusMessage finalMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                };
                await sender.SendMessageAsync(finalMessage);
            }
            );
        }
    }
}
