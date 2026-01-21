using System.Text;
using System.Text.Json;
using Application.Dto.Request;
using Domain.Interfaces;
using RabbitMQ.Client;
using Application.Dto.Result;
using Application.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.MessageBus;

public class RabbitMqConsumer : IMessageBusConsumer, IAsyncDisposable
{
    private readonly IConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqConsumer(IConfiguration configuration)
    {
        _factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:HostName"] ?? "host.docker.internal",
            Port = int.TryParse(configuration["RabbitMq:Port"], out var port) ? port : 5672,
            UserName = configuration["RabbitMq:UserName"] ?? "guest",
            Password = configuration["RabbitMq:Password"] ?? "guest",
            VirtualHost = configuration["RabbitMq:VirtualHost"] ?? "/",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
    }

    private async Task<IConnection> GetConnectionAsync()
    {
        if (_connection is not null)
            return _connection;

        _connection = await _factory.CreateConnectionAsync();
        return _connection;
    }

    public async Task PublishAsync<T>(
        string exchangeName,
        string queueName,
        string routingKey)
    {
        var connection = await GetConnectionAsync();
        _channel = await connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey
        );

       

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            ContentEncoding = "utf-8"
        };

        await _channel.BasicQosAsync(
            prefetchSize:0,
            prefetchCount:1,
            global:false
            );

        var consumer = new RabbitMQ.Client.Events.AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
            {
        try
        {
           
                var body = ea.Body.ToArray();
                var json = System.Text.Encoding.UTF8.GetString(body);
                var msg = System.Text.Encoding.UTF8.GetString(body);
                Console.WriteLine($"Mensagem Recebida -> {msg}");

                switch (routingKey)
                {
                    case "PaymentProcessedEvent":
                        var payment = JsonSerializer.Deserialize<PaymentStatusDto>(json);

                
                        if (payment != null)
                            new SendEmailService().sendEmaiPayment(payment); 

                        break;
                    case "UserCreatedEvent":
                        var userRegistered = JsonSerializer.Deserialize<UserRegisterResultDto>(json);
                
                        if (userRegistered!= null && userRegistered?.Email != null && userRegistered?.Nome != null)
                            new SendEmailService().sendEmailWelcome(userRegistered?.Nome, userRegistered?.Email); 

                        break;
                }
                       await Task.Delay(2000);
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
           
        }
        catch (Exception e)
        {
            Console.WriteLine($"Erro ao processar mensagem {e.Message}");
            await Task.Delay(2000);
            await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue:false);

        }
            };
        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer:consumer
        );
        Console.WriteLine("Aguardando próxima mensagem.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
