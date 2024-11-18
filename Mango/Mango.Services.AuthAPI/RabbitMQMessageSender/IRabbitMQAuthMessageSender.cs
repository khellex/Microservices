namespace Mango.Services.AuthAPI.RabbitMQMessageSender
{
    public interface IRabbitMQAuthMessageSender
    {
        void SendMessage(object message, string queueName);
    }
}
