namespace Domain.Interfaces;

public interface IMessageBusConsumer
{
    Task PublishAsync<T>( string exchangeName, string queueName, string routingKey);
}