using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.DTOs;
using PaymentService.Models;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(PaymentDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll()
    {
        var payments = await dbContext.Payments.ToListAsync();
        return Ok(payments.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaymentDto>> GetById(int id)
    {
        var payment = await dbContext.Payments.FindAsync(id);
        return payment is null ? NotFound() : Ok(ToDto(payment));
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create(CreatePaymentDto request)
    {
        var payment = new Payment
        {
            OrderId = request.OrderId,
            Amount = request.Amount,
            Status = request.Status
        };

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, ToDto(payment));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdatePaymentDto request)
    {
        var payment = await dbContext.Payments.FindAsync(id);
        if (payment is null)
        {
            return NotFound();
        }

        payment.OrderId = request.OrderId;
        payment.Amount = request.Amount;
        payment.Status = request.Status;

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

    private static PaymentDto ToDto(Payment payment) => new()
    {
        Id = payment.Id,
        OrderId = payment.OrderId,
        Amount = payment.Amount,
        Status = payment.Status
    };
}
