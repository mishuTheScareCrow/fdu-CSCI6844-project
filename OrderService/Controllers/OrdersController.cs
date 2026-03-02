using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using System.Net;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(OrderDbContext dbContext, IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll()
    {
        var orders = await dbContext.Orders.Include(o => o.Items).ToListAsync();
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Order>> GetById(int id)
    {
        var order = await dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create(Order order)
    {
        var customerClient = httpClientFactory.CreateClient("CustomerService");
        using var customerResponse = await customerClient.GetAsync($"api/customers/{order.CustomerId}");

        if (customerResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return BadRequest($"Customer {order.CustomerId} does not exist.");
        }

        if (!customerResponse.IsSuccessStatusCode)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Customer service unavailable.");
        }

        var productClient = httpClientFactory.CreateClient("ProductService");
        foreach (var item in order.Items)
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

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Order order)
    {
        if (id != order.Id)
        {
            return BadRequest();
        }

        var customerClient = httpClientFactory.CreateClient("CustomerService");
        using var customerResponse = await customerClient.GetAsync($"api/customers/{order.CustomerId}");

        if (customerResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return BadRequest($"Customer {order.CustomerId} does not exist.");
        }

        if (!customerResponse.IsSuccessStatusCode)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Customer service unavailable.");
        }

        var productClient = httpClientFactory.CreateClient("ProductService");
        foreach (var item in order.Items)
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

        existing.CustomerId = order.CustomerId;
        existing.Total = order.Total;
        existing.Status = order.Status;
        existing.Items = order.Items.Select(i => new OrderItem { ProductId = i.ProductId, Quantity = i.Quantity }).ToList();

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
}
