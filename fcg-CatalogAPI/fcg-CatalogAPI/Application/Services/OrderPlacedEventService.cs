using Application.Dto.Request;
using Application.Dto.Result;
using Application.Interfaces.Service;


namespace Application.Services;
using Domain.Interfaces;
public class OrderPlacedEventService:IMessageService
{
    private readonly IMessageBusProducer _messageBus;
    private readonly string _exchangeName = "fcgExchange";
    private readonly string _queueName = "PaymentProcessQueue";
    private readonly string _routingKey = "OrderPlacedEvent,";
    public OrderPlacedEventService(IMessageBusProducer messageBus)
    {
        _messageBus = messageBus;
    }

    public async Task Handle(OrderPlacedEventRequestDto message)
    {
        await _messageBus.PublishAsync(message, _exchangeName, _queueName, _routingKey );
    }
}