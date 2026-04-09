namespace OrderService.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = [];
}

public class OrderItemDto
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CreateOrderDto
{
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<CreateOrderItemDto> Items { get; set; } = [];
}

public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateOrderDto
{
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<UpdateOrderItemDto> Items { get; set; } = [];
}

public class UpdateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
