using LaundryManagement.Application.Commands.Clients;
using LaundryManagement.Application.DTOs.Clients;
using LaundryManagement.Application.Queries.Clients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryManagement.API.Controllers;

/// <summary>
/// Controlador para operaciones CRUD de clientes
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(
        IMediator mediator,
        ILogger<ClientesController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Queries (Read Operations)

    /// <summary>
    /// GET /api/clientes - Lista todos los clientes con filtros opcionales
    /// </summary>
    /// <param name="search">Término de búsqueda (nombre, teléfono, número de cliente)</param>
    /// <param name="isActive">Filtrar por estado activo (null = todos)</param>
    /// <param name="sortBy">Campo de ordenamiento: name | createdAt</param>
    /// <param name="sortOrder">Dirección de ordenamiento: asc | desc</param>
    /// <response code="200">Lista de clientes (puede estar vacía)</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ClientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortOrder = "asc")
    {
        var query = new GetAllClientsQuery
        {
            Search = search,
            IsActive = isActive,
            SortBy = sortBy,
            SortOrder = sortOrder
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/clientes/{id} - Obtiene un cliente por ID
    /// </summary>
    /// <param name="id">ID del cliente</param>
    /// <response code="200">Cliente encontrado</response>
    /// <response code="404">Cliente no encontrado</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetClientByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"Cliente con ID {id} no encontrado" });

        return Ok(result);
    }

    #endregion

    #region Commands (Write Operations)

    /// <summary>
    /// POST /api/clientes - Crea un nuevo cliente
    /// </summary>
    /// <param name="command">Datos del cliente a crear</param>
    /// <response code="201">Cliente creado exitosamente</response>
    /// <response code="400">Datos inválidos o regla de negocio violada</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateClientResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClientCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result
        );
    }

    /// <summary>
    /// PUT /api/clientes/{id} - Actualiza un cliente existente
    /// </summary>
    /// <param name="id">ID del cliente a actualizar</param>
    /// <param name="command">Datos actualizados</param>
    /// <response code="200">Cliente actualizado exitosamente</response>
    /// <response code="400">Datos inválidos o regla de negocio violada</response>
    /// <response code="404">Cliente no encontrado</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UpdateClientResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientCommand command)
    {
        if (id != command.ClientId)
            return BadRequest(new { message = "El ID de la URL no coincide con el ID del comando" });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// DELETE /api/clientes/{id} - Desactiva un cliente (soft delete)
    /// </summary>
    /// <param name="id">ID del cliente a desactivar</param>
    /// <response code="204">Cliente desactivado exitosamente</response>
    /// <response code="404">Cliente no encontrado</response>
    /// <response code="409">No se puede desactivar (tiene saldo pendiente)</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeactivateClientCommand(id);
        await _mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// PATCH /api/clientes/{id}/estado - Alterna el estado activo/inactivo del cliente
    /// </summary>
    /// <param name="id">ID del cliente</param>
    /// <response code="200">Estado alternado exitosamente</response>
    /// <response code="404">Cliente no encontrado</response>
    /// <response code="409">No se puede desactivar (tiene saldo pendiente)</response>
    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(typeof(ToggleClientStatusResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var command = new ToggleClientStatusCommand(id);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    #endregion
}
