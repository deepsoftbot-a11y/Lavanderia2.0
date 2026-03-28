using LaundryManagement.Application.Commands.ServiceGarments;
using LaundryManagement.Application.DTOs.ServiceGarments;
using LaundryManagement.Application.Queries.ServiceGarments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryManagement.API.Controllers;

[ApiController]
[Route("api/garment-types")]
[Authorize]
[Produces("application/json")]
public class TiposPrendaController : ControllerBase
{
    private readonly IMediator _mediator;

    public TiposPrendaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Queries (CQRS - Read Side)

    /// <summary>
    /// Obtiene todos los tipos de prenda con filtros opcionales
    /// </summary>
    /// <param name="search">Texto de búsqueda</param>
    /// <param name="isActive">Filtrar por estado activo</param>
    /// <returns>Lista de tipos de prenda</returns>
    /// <response code="200">Lista de tipos de prenda (puede estar vacía)</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ServiceGarmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServiceGarments(
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetServiceGarmentsQuery
        {
            Search = search,
            IsActive = isActive
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un tipo de prenda por su ID
    /// </summary>
    /// <param name="id">ID del tipo de prenda</param>
    /// <returns>Detalles del tipo de prenda</returns>
    /// <response code="200">Tipo de prenda encontrado</response>
    /// <response code="404">Tipo de prenda no encontrado</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ServiceGarmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServiceGarmentById(int id)
    {
        var query = new GetServiceGarmentByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"Tipo de prenda con ID {id} no encontrado" });

        return Ok(result);
    }

    #endregion

    #region Commands (CQRS - Write Side)

    /// <summary>
    /// Crea un nuevo tipo de prenda
    /// </summary>
    /// <param name="command">Datos del tipo de prenda a crear</param>
    /// <returns>ID del tipo de prenda creado</returns>
    /// <response code="201">Tipo de prenda creado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateServiceGarment([FromBody] CreateServiceGarmentCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var garmentId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetServiceGarmentById), new { id = garmentId }, new { id = garmentId });
    }

    /// <summary>
    /// Actualiza un tipo de prenda existente
    /// </summary>
    /// <param name="id">ID del tipo de prenda</param>
    /// <param name="command">Datos actualizados del tipo de prenda</param>
    /// <returns>Confirmación de actualización</returns>
    /// <response code="200">Tipo de prenda actualizado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Tipo de prenda no encontrado</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateServiceGarment(int id, [FromBody] UpdateServiceGarmentCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedCommand = command with { ServiceGarmentId = id };
        await _mediator.Send(updatedCommand);
        return Ok(new { message = "Tipo de prenda actualizado exitosamente" });
    }

    /// <summary>
    /// Elimina un tipo de prenda
    /// </summary>
    /// <param name="id">ID del tipo de prenda</param>
    /// <returns>Confirmación de eliminación</returns>
    /// <response code="200">Tipo de prenda eliminado exitosamente</response>
    /// <response code="404">Tipo de prenda no encontrado</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteServiceGarment(int id)
    {
        var command = new DeleteServiceGarmentCommand(id);
        await _mediator.Send(command);
        return Ok(new { message = "Tipo de prenda eliminado exitosamente" });
    }

    /// <summary>
    /// Activa o desactiva un tipo de prenda
    /// </summary>
    /// <param name="id">ID del tipo de prenda</param>
    /// <param name="isActive">Nuevo estado (true = activo, false = inactivo)</param>
    /// <returns>Confirmación de cambio de estado</returns>
    /// <response code="200">Estado del tipo de prenda actualizado</response>
    /// <response code="404">Tipo de prenda no encontrado</response>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleServiceGarmentStatus(int id, [FromQuery] bool isActive)
    {
        var command = new ToggleServiceGarmentStatusCommand { ServiceGarmentId = id, IsActive = isActive };
        await _mediator.Send(command);
        return Ok(new { message = $"Tipo de prenda {(isActive ? "activado" : "desactivado")} exitosamente" });
    }

    #endregion
}
