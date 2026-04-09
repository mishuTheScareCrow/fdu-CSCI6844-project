using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Messaging;
using OrderService.Models;
using System.Net;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(OrderDbContext dbContext, IHttpClientFactory httpClientFactory, IOrderEventPublisher orderEventPublisher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        var orders = await dbContext.Orders.Include(o => o.Items).ToListAsync();
        return Ok(orders.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        return order is null ? NotFound() : Ok(ToDto(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderDto request)
    {
        var customerClient = httpClientFactory.CreateClient("CustomerService");
        using var customerResponse = await customerClient.GetAsync($"api/customers/{request.CustomerId}");

        if (customerResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return BadRequest($"Customer {request.CustomerId} does not exist.");
        }

        if (!customerResponse.IsSuccessStatusCode)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Customer service unavailable.");
        }

        var productClient = httpClientFactory.CreateClient("ProductService");
        foreach (var item in request.Items)
        {
            using var productResponse = await productClient.GetAsync($"api/products/{item.ProductId}");

            if (productResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return BadRequest($"Product {item.ProductId} does not exist.");
            }

            if (!productResponse.IsSuccessStatusCode)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Product service unavailable.");
            }
        }

        var order = new Order
        {
            CustomerId = request.CustomerId,
            Total = request.Total,
            Status = request.Status,
            Items = request.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        };

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        await orderEventPublisher.PublishOrderCreatedAsync(new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Total = order.Total,
            Status = order.Status,
            Items = order.Items.Select(i => new OrderCreatedItemEvent
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        }, HttpContext.RequestAborted);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, ToDto(order));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateOrderDto request)
    {
        var customerClient = httpClientFactory.CreateClient("CustomerService");
        using var customerResponse = await customerClient.GetAsync($"api/customers/{request.CustomerId}");

        if (customerResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return BadRequest($"Customer {request.CustomerId} does not exist.");
        }

        if (!customerResponse.IsSuccessStatusCode)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Customer service unavailable.");
        }

        var productClient = httpClientFactory.CreateClient("ProductService");
        foreach (var item in request.Items)
        {
            using var productResponse = await productClient.GetAsync($"api/products/{item.ProductId}");

            if (productResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return BadRequest($"Product {item.ProductId} does not exist.");
            }

            if (!productResponse.IsSuccessStatusCode)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Product service unavailable.");
            }
        }

        var existing = await dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.CustomerId = request.CustomerId;
        existing.Total = request.Total;
        existing.Status = request.Status;
        existing.Items = request.Items.Select(i => new OrderItem { ProductId = i.ProductId, Quantity = i.Quantity }).ToList();

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await dbContext.Orders.FindAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        dbContext.Orders.Remove(order);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private static OrderDto ToDto(Order order) => new()
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        Total = order.Total,
        Status = order.Status,
        Items = order.Items.Select(i => new OrderItemDto
        {
            OrderItemId = i.OrderItemId,
            OrderId = i.OrderId,
            ProductId = i.ProductId,
            Quantity = i.Quantity
        }).ToList()
    };
}
