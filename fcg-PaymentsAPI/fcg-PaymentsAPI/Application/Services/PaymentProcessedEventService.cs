
using Application.Dto.Request;
using Application.Dto.Result;
using Application.Interfaces.Service;
using Domain.Interfaces;
using Microsoft.Extensions.Hosting;


namespace Application.Services;
using Domain.Interfaces;

public class PaymentProcessedEventService : IMessageServiceProducer
{
    private readonly IMessageBusProducer _messageBus;
    private readonly string _exchangeName = "fcgExchange";
    private readonly string _queueName = "PaymentProcessQueue";
    private readonly string _routingKey = "PaymentProcessedEvent";

    public PaymentProcessedEventService(IMessageBusProducer messageBus)
    {
        _messageBus = messageBus;
    }
    

    public async Task Handle(PaymentStatusDto message)
    {
        await _messageBus.PublishAsync(message, _exchangeName, _queueName, _routingKey );
    }
    

}