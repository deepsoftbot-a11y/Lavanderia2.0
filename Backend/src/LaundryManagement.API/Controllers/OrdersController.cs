using LaundryManagement.Application.Commands.Orders;
using LaundryManagement.Application.DTOs.Orders;
using LaundryManagement.Application.Queries.Orders;
using LaundryManagement.Application.Queries.Payments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryManagement.API.Controllers;

/// <summary>
/// Controlador de órdenes — API unificada para el frontend.
/// Ruta base: /api/orders (inglés, consistente con el frontend).
/// </summary>
[ApiController]
[Route("api/orders")]
[Authorize]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista órdenes con filtros opcionales
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? search,
        [FromQuery] int? clientId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortOrder = "desc")
    {
        var query = new GetOrdersQuery(search, clientId, startDate, endDate, sortBy, sortOrder);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Busca órdenes por texto (folio, nombre o teléfono de cliente)
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchOrders([FromQuery] string query)
    {
        var result = await _mediator.Send(new SearchOrdersQuery(query));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene una orden por su ID con cliente y pagos poblados
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var result = await _mediator.Send(new GetOrderResponseByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Actualiza datos editables de una orden (fecha prometida, notas, ubicación, items)
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Forzar que el ID del path coincida con el comando
        var commandWithId = command with { Id = id };
        var result = await _mediator.Send(commandWithId);
        return Ok(result);
    }

    /// <summary>
    /// Cancela una orden (borrado lógico — cambia estado a Cancelada)
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(int id, [FromQuery] int cancelledBy = 1)
    {
        await _mediator.Send(new CancelOrderCommand(id, cancelledBy));
        return NoContent();
    }

    /// <summary>
    /// Recalcula y retorna los totales de pago actualizados de una orden
    /// </summary>
    [HttpPatch("{id:int}/payment-totals")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderPaymentTotals(int id)
    {
        var result = await _mediator.Send(new UpdateOrderPaymentTotalsCommand(id));
        return Ok(result);
    }

    /// <summary>
    /// Retorna los pagos activos de una orden
    /// </summary>
    [HttpGet("{id:int}/payments")]
    [ProducesResponseType(typeof(List<OrderPaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentsByOrderId(int id)
    {
        var payments = await _mediator.Send(new GetPaymentsByOrderIdQuery(id));
        return Ok(payments);
    }

    /// <summary>
    /// Cambia el estado de una orden
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(new UpdateOrderStatusCommand(id, request.OrderStatusId, request.ChangedBy));
        return Ok(result);
    }

}

/// <summary>Request body para cambio de estado</summary>
public sealed record UpdateOrderStatusRequest(int OrderStatusId, int ChangedBy = 1);
