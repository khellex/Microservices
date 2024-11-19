using AutoMapper.Internal.Mappers;
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    //this consumer is created as a background service
    public class RabbitMQOrderConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection _connection;
        private IModel _channel;
        string queueName = "";

        public RabbitMQOrderConsumer(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = ConnectionFactory.DefaultUser,
                Password = ConnectionFactory.DefaultPass,
                Port = AmqpTcpEndpoint.UseDefaultPort,
            };
            _connection = factory.CreateConnection();

            _channel = _connection.CreateModel();

            //since now we are listening to a fanout exchange, we will declare a exchange here
            _channel.ExchangeDeclare(_configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic"), ExchangeType.Fanout);

            //when we create an exchange, rabbitmq can cretae its own default queue and use it
            queueName = _channel.QueueDeclare().QueueName;

            //bind this queue to the exchange channel
            _channel.QueueBind(queueName, _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic"), "");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //if cancellation is requested, the processor will stop listening 
            stoppingToken.ThrowIfCancellationRequested();

            #region Order mail Consumer
            //for email and logging order cart
            var mailOrderConsumer = new EventingBasicConsumer(_channel);

            mailOrderConsumer.Received += async (ch, ea) =>
            {
                //we can extract the body from the ea and then extract the email from it
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                RewardsMessage? rewards = JsonConvert.DeserializeObject<RewardsMessage>(content);

                //when the email is fetched, we can pass it to the HandleMessage method 
                await _emailService.LogPlacedOrder(rewards);

                //once we receive the message, we need to send the acknowledgement
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queueName, false, mailOrderConsumer);
            #endregion

            return Task.CompletedTask;
        }

        private async Task HandleMessage(string email)
        {
            await _emailService.EmailUserRegistrationAndLog(email);
        }
    }
}
