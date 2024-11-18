using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    //this consumer is created as a background service
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQConsumer(IConfiguration configuration, EmailService emailService)
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

            //on the same channel we can declare two queues, UserRegistrationQueue
            _channel.QueueDeclare(_configuration.GetValue<string>("TopicAndQueueNames:UserRegistrationQueue"), false, false, false, null);

            //EmailShoppingCartQueue
            _channel.QueueDeclare(_configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue"), false, false, false, null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //if cancellation is requested, the processor will stop listening 
            stoppingToken.ThrowIfCancellationRequested();

            #region User Registration consumer
            //initializing the User Registration consumer
            var userRegistrationConsumer = new EventingBasicConsumer(_channel);

            //when the consumer receives an event for delivery, ch:channel, ea: event arguments
            userRegistrationConsumer.Received += async (ch, ea) =>
            {
                //we can extract the body from the ea and then extract the email from it
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                string? email = JsonConvert.DeserializeObject<string>(content);

                //when the email is fetched, we can pass it to the HandleMessage method 
                await HandleMessage(email);

                //once we receive the message, we need to send the acknowledgement
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            //we need to assign the above event handler to the consumer, we use BasicConsume since our queue is a basic one
            //we set the autoAck to false, since on line 54 we have mentioned a manual acknowledgement
            _channel.BasicConsume(_configuration.GetValue<string>("TopicAndQueueNames:UserRegistrationQueue"), false, userRegistrationConsumer);
            #endregion

            #region Mail Cart Order Consumer
            //for email and logging order cart
            var mailCartOrderConsumer = new EventingBasicConsumer(_channel);

            mailCartOrderConsumer.Received += async (ch, ea) =>
            {
                //we can extract the body from the ea and then extract the email from it
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                CartDto? cart = JsonConvert.DeserializeObject<CartDto>(content);

                //when the email is fetched, we can pass it to the HandleMessage method 
                await _emailService.EmailCartAndLog(cart);

                //once we receive the message, we need to send the acknowledgement
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(_configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue"), false, mailCartOrderConsumer);
            #endregion

            return Task.CompletedTask;
        }

        private async Task HandleMessage(string email)
        {
            await _emailService.EmailUserRegistrationAndLog(email);
        }
    }
}
