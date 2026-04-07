using LaundryManagement.Application.Commands.Services;
using LaundryManagement.Application.DTOs.Services;
using LaundryManagement.Application.Queries.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryManagement.API.Controllers;

[ApiController]
[Route("api/Services")]
[Authorize]
[Produces("application/json")]
public class ServiciosController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServiciosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Queries (CQRS - Read Side)

    /// <summary>
    /// Obtiene todos los servicios con filtros opcionales
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ServiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServices(
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? chargeType = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetServicesQuery
        {
            Search = search,
            CategoryId = categoryId,
            ChargeType = chargeType,
            IsActive = isActive
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un servicio por su ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServiceById(int id)
    {
        var query = new GetServiceByIdQuery { ServiceId = id };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"Servicio con ID {id} no encontrado" });

        return Ok(result);
    }

    #endregion

    #region Commands (CQRS - Write Side)

    /// <summary>
    /// Crea un nuevo servicio
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateService([FromBody] CreateServiceCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetServiceById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Actualiza un servicio existente
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateService(int id, [FromBody] UpdateServiceCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedCommand = command with { ServiceId = id };
        var result = await _mediator.Send(updatedCommand);
        return Ok(result);
    }

    /// <summary>
    /// Elimina un servicio
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteService(int id)
    {
        var command = new DeleteServiceCommand { ServiceId = id };
        await _mediator.Send(command);
        return Ok(new { message = "Servicio eliminado exitosamente" });
    }

    /// <summary>
    /// Activa o desactiva un servicio
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleServiceStatus(int id, [FromQuery] bool isActive)
    {
        var command = new ToggleServiceStatusCommand { ServiceId = id, IsActive = isActive };
        await _mediator.Send(command);

        var updated = await _mediator.Send(new GetServiceByIdQuery { ServiceId = id });
        return Ok(updated);
    }

    /// <summary>
    /// Agrega un precio específico para un tipo de prenda al servicio (legacy endpoint)
    /// </summary>
    [HttpPost("{id:int}/precios")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddServicePrice(int id, [FromBody] AddServicePriceCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedCommand = command with { ServiceId = id };
        var priceId = await _mediator.Send(updatedCommand);
        return CreatedAtAction("GetServicePriceById", "ServiciosPrendas", new { id = priceId }, new { id = priceId });
    }

    #endregion
}
