using LaundryManagement.Application.Commands.Payments;
using LaundryManagement.Application.DTOs.Orders;
using LaundryManagement.Application.Queries.Payments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryManagement.API.Controllers;

/// <summary>
/// Gestión de pagos — API orientada al frontend.
/// Ruta base: /api/payments (inglés, consistente con el frontend).
/// </summary>
[ApiController]
[Route("api/payments")]
[Authorize]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Registra un nuevo pago para una orden
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderPaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var payment = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, payment);
    }

    /// <summary>
    /// Obtiene un pago por su ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderPaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentById(int id)
    {
        var payment = await _mediator.Send(new GetPaymentByIdQuery(id));
        return Ok(payment);
    }

    /// <summary>
    /// Cancela (borrado lógico) un pago existente
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelPayment(int id, [FromQuery] int cancelledBy = 1)
    {
        await _mediator.Send(new CancelPaymentCommand(id, cancelledBy));
        return NoContent();
    }
}
