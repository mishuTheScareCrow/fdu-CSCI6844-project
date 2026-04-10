using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductService.Messaging;

public sealed class OrderCreatedConsumer(
    RabbitMqOptions options,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<OrderCreatedConsumer> logger) : BackgroundService
{
    private readonly RabbitMqOptions options = options;
    private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<OrderCreatedConsumer> logger = logger;

    private IConnection? connection;
    private IModel? channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connected = false;
        while (!stoppingToken.IsCancellationRequested && !connected)
        {
            try
            {
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
                channel.QueueDeclare(options.OrderCreatedQueueName, durable: true, exclusive: false, autoDelete: false);
                channel.QueueBind(options.OrderCreatedQueueName, options.ExchangeName, string.Empty);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += OnOrderCreatedAsync;

                channel.BasicConsume(options.OrderCreatedQueueName, autoAck: false, consumer: consumer);
                logger.LogInformation("OrderCreatedConsumer started. Queue: {Queue}", options.OrderCreatedQueueName);
                connected = true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "RabbitMQ not ready yet. Retrying in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task OnOrderCreatedAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        if (channel is null)
        {
            return;
        }

        try
        {
            var payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            var message = JsonSerializer.Deserialize<OrderCreatedEvent>(payload);

            if (message is null)
            {
                channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            using var scope = serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

            var hasChanges = false;
            foreach (var item in message.Items)
            {
                var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (product is null)
                {
                    continue;
                }

                product.Stock = Math.Max(0, product.Stock - item.Quantity);
                hasChanges = true;
            }

            if (hasChanges)
            {
                await dbContext.SaveChangesAsync();
            }

            logger.LogInformation("Processed OrderCreated event for OrderId {OrderId}", message.OrderId);
            channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process OrderCreated event.");
            channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
        }
    }

    public override void Dispose()
    {
        channel?.Dispose();
        connection?.Dispose();
        base.Dispose();
    }
}
