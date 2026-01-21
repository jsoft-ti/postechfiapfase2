namespace Infrastructure.MessageBus;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "host.docker.internal";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}
