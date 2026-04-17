using System.Net;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCorsPolicy";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5138", "https://localhost:7249")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddHttpClient("OrderService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:OrderService"] ?? "http://localhost:5003/");
});
builder.Services.AddHttpClient("CustomerService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:CustomerService"] ?? "http://localhost:5001/");
});
builder.Services.AddHttpClient("ProductService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ProductService"] ?? "http://localhost:5002/");
});
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors(FrontendCorsPolicy);

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.ConfigObject.Urls =
    [
        new UrlDescriptor { Name = "API Gateway", Url = "/swagger/v1/swagger.json" },
        new UrlDescriptor { Name = "Customer Service", Url = "/docs/customers/swagger/v1/swagger.json" },
        new UrlDescriptor { Name = "Product Service", Url = "/docs/products/swagger/v1/swagger.json" },
        new UrlDescriptor { Name = "Order Service", Url = "/docs/orders/swagger/v1/swagger.json" },
        new UrlDescriptor { Name = "Payment Service", Url = "/docs/payments/swagger/v1/swagger.json" }
    ];
});

app.MapGet("/api/orders/{id:int}/details", async (int id, IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
{
    var orderClient = httpClientFactory.CreateClient("OrderService");
    using var orderResponse = await orderClient.GetAsync($"api/orders/{id}", cancellationToken);

    if (orderResponse.StatusCode == HttpStatusCode.NotFound)
    {
        return Results.NotFound();
    }

    if (!orderResponse.IsSuccessStatusCode)
    {
        return Results.StatusCode(StatusCodes.Status502BadGateway);
    }

    var order = await orderResponse.Content.ReadFromJsonAsync<OrderResponse>(cancellationToken: cancellationToken);
    if (order is null)
    {
        return Results.StatusCode(StatusCodes.Status502BadGateway);
    }

    var customerClient = httpClientFactory.CreateClient("CustomerService");
    using var customerResponse = await customerClient.GetAsync($"api/customers/{order.CustomerId}", cancellationToken);

    if (!customerResponse.IsSuccessStatusCode)
    {
        return Results.StatusCode(StatusCodes.Status502BadGateway);
    }

    var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerResponse>(cancellationToken: cancellationToken);
    if (customer is null)
    {
        return Results.StatusCode(StatusCodes.Status502BadGateway);
    }

    var productClient = httpClientFactory.CreateClient("ProductService");
    var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();

    var productTasks = productIds.Select(async productId =>
    {
        using var productResponse = await productClient.GetAsync($"api/products/{productId}", cancellationToken);
        if (!productResponse.IsSuccessStatusCode)
        {
            return (ProductResponse?)null;
        }

        return await productResponse.Content.ReadFromJsonAsync<ProductResponse>(cancellationToken: cancellationToken);
    });

    var products = (await Task.WhenAll(productTasks))
        .Where(p => p is not null)
        .Cast<ProductResponse>()
        .ToList();

    return Results.Ok(new OrderDetailsResponse
    {
        Order = order,
        Customer = customer,
        Products = products
    });
});

app.MapReverseProxy();

app.Run();

public class OrderDetailsResponse
{
    public required OrderResponse Order { get; set; }
    public required CustomerResponse Customer { get; set; }
    public required List<ProductResponse> Products { get; set; }
}

public class OrderResponse
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItemResponse> Items { get; set; } = [];
}

public class OrderItemResponse
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CustomerResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
