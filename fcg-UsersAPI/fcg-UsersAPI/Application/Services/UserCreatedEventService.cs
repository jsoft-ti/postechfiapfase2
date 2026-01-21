using Domain.Entities;
using Application.Interfaces.Service;
using Domain.Interfaces;

namespace Application.Services;


public class UserCreatedEventService:IMessageService
{
    private readonly IMessageBusProducer _messageBus;
    private readonly string _exchangeName = "fcgExchange";
    private readonly string _queueName = "UserCreateNotificationQueue";
    private readonly string _routingKey = "UserCreatedEvent";
    public UserCreatedEventService(IMessageBusProducer messageBus)
    {
        _messageBus = messageBus;
    }

    public async Task Handle(UserRegisterResultDto message)
    {
        await _messageBus.PublishAsync<string>(message, _exchangeName, _queueName, _routingKey );
    }
}