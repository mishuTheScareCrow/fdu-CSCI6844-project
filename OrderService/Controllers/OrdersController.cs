using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(OrderDbContext dbContext) : ControllerBase
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
