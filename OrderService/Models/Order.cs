namespace OrderService.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = [];
}

public class OrderItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
