using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace OrderService.Messaging;

public sealed class RabbitMqOrderEventPublisher : IOrderEventPublisher, IDisposable
{
    private readonly RabbitMqOptions options;
    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly SemaphoreSlim publishLock = new(1, 1);

    public RabbitMqOrderEventPublisher(RabbitMqOptions options)
    {
        this.options = options;

        var factory = new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = options.VirtualHost,
            DispatchConsumersAsync = true
        };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        channel.ExchangeDeclare(options.ExchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);
    }

    public async Task PublishOrderCreatedAsync(OrderCreatedEvent orderCreatedEvent, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(orderCreatedEvent);
        var body = Encoding.UTF8.GetBytes(payload);

        await publishLock.WaitAsync(cancellationToken);
        try
        {
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(
                exchange: options.ExchangeName,
                routingKey: string.Empty,
                basicProperties: properties,
                body: body);
        }
        finally
        {
            publishLock.Release();
        }
    }

    public void Dispose()
    {
        channel.Dispose();
        connection.Dispose();
        publishLock.Dispose();
    }
}
