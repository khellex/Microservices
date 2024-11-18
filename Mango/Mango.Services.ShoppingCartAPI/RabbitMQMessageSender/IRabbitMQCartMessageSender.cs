namespace Mango.Services.ShoppingCartAPI.RabbitMQMessageSender
{
    public interface IRabbitMQCartMessageSender
    {
        void SendMessage(object message, string queueName);
    }
}
