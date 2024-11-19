namespace Mango.Services.ShoppingCartAPI.RabbitMQMessageSender
{
    public interface IRabbitMQOrderMessageSender
    {
        void SendMessage(object message, string exchangeName);
    }
}
