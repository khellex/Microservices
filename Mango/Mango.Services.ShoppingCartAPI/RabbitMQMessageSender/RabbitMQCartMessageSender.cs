using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.ShoppingCartAPI.RabbitMQMessageSender
{
    public class RabbitMQCartMessageSender : IRabbitMQCartMessageSender
    {
        private IConnection _connection;

        public void SendMessage(object message, string queueName)
        {
            if (ConnectionExists()) // true is conn exists, if false make new conn
            {
                //based on that connection, now we need to set up a channel so we can communicate with the message queue
                using var channel = _connection.CreateModel();

                //based on this channel, now we need to configure a queue
                channel.QueueDeclare(queueName, false, false, false, null);

                //we serialize and encode the message before publishing it on the queue
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                //this is used to finally publish the message tot he rabbitmq service bus,
                //here we set exchange:"" since we need to send the message directly to the user
                channel.BasicPublish(exchange: "", routingKey: queueName, null, body: body);
            }
        }
        //implemented this to make sure we only make a single connection
        //while sending the message, rather than establishing a new conn every time
        private void CreateConnection()
        {
            try
            {
                //for rabbit mq we need to first set up the conn factory with the hostname, username and password
                var factory = new ConnectionFactory
                {
                    HostName = "localhost",
                    UserName = ConnectionFactory.DefaultUser,
                    Password = ConnectionFactory.DefaultPass,
                    Port = AmqpTcpEndpoint.UseDefaultPort,
                };

                //then we need to establish the connection
                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool ConnectionExists()
        {
            if (_connection == null)
            {
                CreateConnection();
                return true;
            }
            return true;
        }
    }
}
