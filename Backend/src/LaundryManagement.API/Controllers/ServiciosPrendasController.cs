using LaundryManagement.Application.Commands.ServicePrices;
using LaundryManagement.Application.Commands.Services;
using LaundryManagement.Application.DTOs.ServicePrices;
using LaundryManagement.Application.Queries.ServicePrices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryManagement.API.Controllers;

[ApiController]
[Route("api/service-garments")]
[Authorize]
[Produces("application/json")]
public class ServiciosPrendasController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServiciosPrendasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Queries (CQRS - Read Side)

    /// <summary>
    /// Obtiene todos los precios de servicios-prendas con filtros opcionales
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ServicePriceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServicePrices(
        [FromQuery] int? serviceId = null,
        [FromQuery] int? serviceGarmentId = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetServicePricesQuery
        {
            ServiceId = serviceId,
            ServiceGarmentId = serviceGarmentId,
            IsActive = isActive
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un precio de servicio-prenda por su ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ServicePriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServicePriceById(int id)
    {
        var query = new GetServicePriceByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"Precio con ID {id} no encontrado" });

        return Ok(result);
    }

    /// <summary>
    /// Obtiene el precio para una combinación específica de servicio y tipo de prenda
    /// </summary>
    [HttpGet("{serviceId:int}/{garmentId:int}")]
    [ProducesResponseType(typeof(ServicePriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServicePriceByCombo(int serviceId, int garmentId)
    {
        var query = new GetServicePriceByComboQuery(serviceId, garmentId);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"No existe precio para el servicio {serviceId} y prenda {garmentId}" });

        return Ok(result);
    }

    #endregion

    #region Commands (CQRS - Write Side)

    /// <summary>
    /// Crea un nuevo precio servicio-prenda
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServicePriceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateServicePrice([FromBody] CreateServicePriceCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetServicePriceById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Actualiza el precio de una combinación servicio-prenda existente
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ServicePriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateServicePrice(int id, [FromBody] UpdateServicePriceBody body)
    {
        if (body.UnitPrice <= 0)
            return BadRequest(new { message = "El precio debe ser mayor que cero" });

        var command = new UpdateServicePriceCommand
        {
            ServicePriceId = id,
            UnitPrice = body.UnitPrice
        };

        await _mediator.Send(command);

        var updated = await _mediator.Send(new GetServicePriceByIdQuery(id));
        return Ok(updated);
    }

    /// <summary>
    /// Elimina un precio de servicio-prenda (desactivación lógica)
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteServicePrice(int id)
    {
        var command = new DeleteServicePriceCommand { ServicePriceId = id };
        await _mediator.Send(command);
        return Ok(new { message = "Precio eliminado exitosamente" });
    }

    /// <summary>
    /// Alterna el estado activo/inactivo de un precio servicio-prenda
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ServicePriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleServicePriceStatus(int id)
    {
        var command = new ToggleServicePriceStatusCommand { ServicePriceId = id };
        await _mediator.Send(command);

        var updated = await _mediator.Send(new GetServicePriceByIdQuery(id));
        return Ok(updated);
    }

    #endregion
}

/// <summary>
/// Body para actualizar el precio de una combinación servicio-prenda
/// </summary>
public sealed record UpdateServicePriceBody(decimal UnitPrice);
