using System.Net.Mime;
using Domain.Entities;

namespace Domain.Interfaces;

public interface IMessageBusProducer
{
    Task PublishAsync<T>(UserRegisterResultDto message, string exchangeName, string queueName, string routingKey);
}