namespace ProductService.Messaging;

public class OrderCreatedEvent
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderCreatedItemEvent> Items { get; set; } = [];
}

public class OrderCreatedItemEvent
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
