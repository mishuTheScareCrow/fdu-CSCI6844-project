// DTO received from API Gateway - matches the response from Product Service
namespace Frontend.Models;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
