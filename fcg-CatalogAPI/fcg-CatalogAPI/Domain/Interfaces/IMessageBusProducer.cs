namespace Domain.Interfaces;

public interface IMessageBusProducer
{
    Task PublishAsync<T>(T message, string exchangeName, string queueName, string routingKey);
}