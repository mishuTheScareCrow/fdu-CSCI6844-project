using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(PaymentDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Payment>>> GetAll()
    {
        var payments = await dbContext.Payments.ToListAsync();
        return Ok(payments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Payment>> GetById(int id)
    {
        var payment = await dbContext.Payments.FindAsync(id);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpPost]
    public async Task<ActionResult<Payment>> Create(Payment payment)
    {
        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Payment payment)
    {
        if (id != payment.Id)
        {
            return BadRequest();
        }

        var existing = await dbContext.Payments.AnyAsync(p => p.Id == id);
        if (!existing)
        {
            return NotFound();
        }

        dbContext.Entry(payment).State = EntityState.Modified;
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var payment = await dbContext.Payments.FindAsync(id);
        if (payment is null)
        {
            return NotFound();
        }

        dbContext.Payments.Remove(payment);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
