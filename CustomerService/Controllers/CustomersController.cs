using CustomerService.Data;
using CustomerService.DTOs;
using CustomerService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController(CustomerDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
    {
        var customers = await dbContext.Customers.ToListAsync();
        return Ok(customers.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var customer = await dbContext.Customers.FindAsync(id);
        return customer is null ? NotFound() : Ok(ToDto(customer));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerDto request)
    {
        var customer = new Customer
        {
            Name = request.Name,
            Email = request.Email
        };

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, ToDto(customer));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCustomerDto request)
    {
        var customer = await dbContext.Customers.FindAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        customer.Name = request.Name;
        customer.Email = request.Email;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await dbContext.Customers.FindAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        dbContext.Customers.Remove(customer);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private static CustomerDto ToDto(Customer customer) => new()
    {
        Id = customer.Id,
        Name = customer.Name,
        Email = customer.Email
    };
}
