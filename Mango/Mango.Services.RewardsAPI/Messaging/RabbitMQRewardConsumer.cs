using Mango.Services.RewardsAPI.Message;
using Mango.Services.RewardsAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.RewardsAPI.Messaging
{
    //this consumer is created as a background service
    public class RabbitMQRewardConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly RewardService _rewardService;
        private IConnection _connection;
        private IModel _channel;
        string queueName = "";

        public RabbitMQRewardConsumer(IConfiguration configuration, RewardService rewardService)
        {
            _configuration = configuration;
            _rewardService = rewardService;

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
                await _rewardService.UpdateRewards(rewards);

                //once we receive the message, we need to send the acknowledgement
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queueName, false, mailOrderConsumer);
            #endregion

            return Task.CompletedTask;
        }
    }
}
