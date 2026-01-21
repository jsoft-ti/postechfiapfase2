using System.Text;
using System.Text.Json;
using Application.Dto.Enum;
using Application.Dto.Request;
using Domain.Interfaces;
using Application.Interfaces.Repository;
using Domain.Entities;
using RabbitMQ.Client;

namespace Infrastructure.MessageBus;

public class RabbitMqConsumer : IMessageBusConsumer, IAsyncDisposable
{
    private readonly IGalleryRepository _galleryRepository;
    private readonly ILibraryRepository _libraryRepository;
    private readonly IConnectionFactory _factory = new ConnectionFactory
    {
        HostName = "localhost",
        Port = 5672,
        UserName = "guest",
        Password = "guest",
        VirtualHost = "/",
        AutomaticRecoveryEnabled = true,
        NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
    };
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqConsumer(IGalleryRepository galleryRepository, ILibraryRepository libraryRepository)
    {
        _galleryRepository = galleryRepository;
        _libraryRepository = libraryRepository;
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
           
                try
                {
                    var body = ea.Body.ToArray();
                    var json = System.Text.Encoding.UTF8.GetString(body);
                    Console.WriteLine($"Mensagem Recebida -> {body}");
                    var psDto = JsonSerializer.Deserialize<PaymentStatusDto>(json);

                    if (psDto.Status == PaymentStatus.Approved)
                    {
                        Console.WriteLine("Pedido aprovado");
                        var galleryGame = await _galleryRepository.GetGalleryGameByIdAsync(psDto.Order.GameId);
                        var libraryGame = new LibraryGame(galleryGame.Id, psDto.Order.UserId, psDto.Order.Price);
                        var result = await _libraryRepository.AddToLibraryAsync(libraryGame); 
                        Console.WriteLine("Jogo Salvo na Galeria");
                        
                    }
                    
                        
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Erro ao resgatar o Json da Order {e.Message}");
                    throw;
                }
                
                    
                //var msg = System.Text.Encoding.UTF8.GetString(body);
               
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
