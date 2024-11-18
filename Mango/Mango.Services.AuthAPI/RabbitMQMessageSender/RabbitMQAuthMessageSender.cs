using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.AuthAPI.RabbitMQMessageSender
{
    public class RabbitMQAuthMessageSender : IRabbitMQAuthMessageSender
    {
        private IConnection _connection;

        public async void SendMessage(object message, string queueName)
        {
            //for rabbit mq we need to first set up the conn factory with the hostname, username and password
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = ConnectionFactory.DefaultUser,
                Password = ConnectionFactory.DefaultPass,
                //Port = AmqpTcpEndpoint.UseDefaultPort,
            };

            //then we need to establish the connection
            _connection = await factory.CreateConnectionAsync();

            //based on that connection, now we need to set up a channel so we can communicate with the message queue
            using var channel = await _connection.CreateChannelAsync();

            //based on this channel, now we need to configure a queue
            await channel.QueueDeclareAsync(queueName, false, false, false, null);

            //we serialize and encode the message before publishing it on the queue
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            //this is used to finally publish the message tot he rabbitmq service bus,
            //here we set exchange:"" since we need to send the message directly to the user
            await channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: body);
        }
    }
}
