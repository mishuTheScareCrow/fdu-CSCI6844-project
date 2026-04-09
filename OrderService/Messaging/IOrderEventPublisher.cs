namespace OrderService.Messaging;

public interface IOrderEventPublisher
{
    Task PublishOrderCreatedAsync(OrderCreatedEvent orderCreatedEvent, CancellationToken cancellationToken = default);
}
