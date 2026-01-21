using System.Text;
using System.Text.Json;
using Domain.Interfaces;
using RabbitMQ.Client;

namespace Infrastructure.MessageBus;

public class RabbitMqProducer : IMessageBusProducer, IAsyncDisposable
{
    private readonly IConnectionFactory _factory;
    private IConnection? _connection;

    public RabbitMqProducer(RabbitMqOptions options)
    {
        _factory = new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = "/",
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
        T message,
        string exchangeName,
        string queueName,
        string routingKey)
    {
        var connection = await GetConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey
        );

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(message)
        );

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            ContentEncoding = "utf-8"
        };

        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body
        );
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
