using Application.Dto.Result;
using Application.Interfaces.Service;
using Domain.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Application.Services;

public class PaymentProcessedEventService : BackgroundService
{
    private readonly IMessageBusConsumer _messageBus;
    private readonly string _exchangeName = "fcgExchange";
    private readonly string _queueName = "PaymentProcessQueue";
    private readonly string _routingKey = "PaymentProcessedEvent";
    public PaymentProcessedEventService(IMessageBusConsumer messageBus)
    {
        _messageBus = messageBus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _messageBus.PublishAsync<string>( _exchangeName, _queueName, _routingKey );
        
        // Mantém o serviço vivo enquanto o token não for cancelado
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }   
}