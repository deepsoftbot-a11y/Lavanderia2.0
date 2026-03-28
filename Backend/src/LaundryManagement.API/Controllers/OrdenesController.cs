using LaundryManagement.Application.Commands.Orders;
using LaundryManagement.Application.DTOs.Ordenes;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Application.Queries.Orders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OrdenesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IOrdenService _ordenService;

    public OrdenesController(IMediator mediator, IOrdenService ordenService)
    {
        _mediator = mediator;
        _ordenService = ordenService;
    }

    #region Queries (CQRS - Read Side)

    /// <summary>
    /// Obtiene una orden por su ID usando DDD (RECOMENDADO)
    /// </summary>
    /// <param name="id">ID de la orden</param>
    /// <returns>Detalles completos de la orden</returns>
    /// <response code="200">Orden encontrada</response>
    /// <response code="404">Orden no encontrada</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"Orden con ID {id} no encontrada" });

        return Ok(result);
    }

    /// <summary>
    /// Obtiene todas las órdenes de un cliente usando DDD (RECOMENDADO)
    /// </summary>
    /// <param name="clientId">ID del cliente</param>
    /// <param name="includeDelivered">Incluir órdenes entregadas (default: true)</param>
    /// <returns>Lista de órdenes del cliente</returns>
    /// <response code="200">Lista de órdenes (puede estar vacía)</response>
    [HttpGet("client/{clientId:int}")]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersByClient(int clientId, [FromQuery] bool includeDelivered = true)
    {
        var query = new GetOrdersByClientQuery(clientId, includeDelivered);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    #endregion

    #region Commands (CQRS - Write Side)

    /// <summary>
    /// Crea una nueva orden de lavandería usando DDD (RECOMENDADO)
    /// </summary>
    /// <param name="command">Comando con los datos de la orden a crear</param>
    /// <returns>Resultado con ID, folio y total de la orden creada</returns>
    /// <response code="200">Orden creada exitosamente</response>
    /// <response code="400">Datos de entrada inválidos o reglas de negocio violadas</response>
    [HttpPost("v2")]
    [ProducesResponseType(typeof(CreateOrderResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    #endregion

    #region Legacy Endpoints (Stored Procedures)

    /// <summary>
    /// Crea una nueva orden de lavandería usando stored procedure (LEGACY)
    /// </summary>
    /// <param name="request">Datos de la orden a crear</param>
    /// <returns>ID de la orden creada</returns>
    /// <response code="200">Orden creada exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    [HttpPost]
    [ProducesResponseType(typeof(CrearOrdenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _ordenService.CrearOrdenAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Cambia el estado de una orden existente
    /// </summary>
    /// <param name="request">Datos para cambiar el estado</param>
    /// <returns>Confirmación del cambio de estado</returns>
    /// <response code="200">Estado cambiado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Orden no encontrada</response>
    [HttpPut("estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado([FromBody] CambiarEstadoOrdenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _ordenService.CambiarEstadoOrdenAsync(request);
        return Ok(new { message = "Estado de la orden actualizado exitosamente" });
    }

    #endregion
}
