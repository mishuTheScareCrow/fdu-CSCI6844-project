using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<OrderDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=order.db";
    options.UseSqlite(connectionString);
});
builder.Services.AddHttpClient("CustomerService", client =>
{
    var baseUrl = builder.Configuration["ServiceUrls:CustomerService"] ?? "http://localhost:5119/";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddHttpClient("ProductService", client =>
{
    var baseUrl = builder.Configuration["ServiceUrls:ProductService"] ?? "http://localhost:5287/";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddSingleton(builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>() ?? new RabbitMqOptions());
builder.Services.AddSingleton<IOrderEventPublisher, RabbitMqOrderEventPublisher>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
