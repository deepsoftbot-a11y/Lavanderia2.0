using LaundryManagement.Application.Commands.Discounts;
using LaundryManagement.Application.DTOs.Discounts;
using LaundryManagement.Application.Queries.Discounts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryManagement.API.Controllers;

/// <summary>
/// Controlador para operaciones CRUD de descuentos del catálogo.
/// Resuelve a /api/descuentos por convención de [controller].
/// </summary>
[ApiController]
[Route("api/Discounts")]
[Authorize]
[Produces("application/json")]
public class DescuentosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DescuentosController> _logger;

    public DescuentosController(
        IMediator mediator,
        ILogger<DescuentosController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger   = logger   ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Queries (Read Operations)

    /// <summary>
    /// GET /api/descuentos - Lista todos los descuentos con filtros opcionales
    /// </summary>
    /// <param name="search">Término de búsqueda en nombre</param>
    /// <param name="tipo">Filtrar por tipo: NONE, PERCENTAGE, FIXED</param>
    /// <param name="activo">Filtrar por estado activo (null = todos)</param>
    /// <param name="ordenarPor">Campo de ordenamiento: name | value</param>
    /// <param name="orden">Dirección: asc | desc</param>
    /// <response code="200">Lista de descuentos (puede estar vacía)</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<DiscountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search    = null,
        [FromQuery] string? tipo      = null,
        [FromQuery] bool?   activo    = null,
        [FromQuery] string  ordenarPor = "name",
        [FromQuery] string  orden     = "asc")
    {
        var query = new GetAllDiscountsQuery
        {
            Search     = search,
            Tipo       = tipo,
            Activo     = activo,
            OrdenarPor = ordenarPor,
            Orden      = orden
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/descuentos/{id} - Obtiene un descuento por ID
    /// </summary>
    /// <param name="id">ID del descuento</param>
    /// <response code="200">Descuento encontrado</response>
    /// <response code="404">Descuento no encontrado</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DiscountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var query  = new GetDiscountByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"Descuento con ID {id} no encontrado" });

        return Ok(result);
    }

    #endregion

    #region Commands (Write Operations)

    /// <summary>
    /// POST /api/descuentos - Crea un nuevo descuento
    /// </summary>
    /// <param name="command">Datos del descuento a crear</param>
    /// <response code="201">Descuento creado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="409">Ya existe un descuento con ese nombre</response>
    [HttpPost]
    [ProducesResponseType(typeof(DiscountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateDiscountCommand command)
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
    /// PUT /api/descuentos/{id} - Actualiza un descuento existente
    /// </summary>
    /// <param name="id">ID del descuento a actualizar</param>
    /// <param name="command">Datos actualizados</param>
    /// <response code="200">Descuento actualizado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="404">Descuento no encontrado</response>
    /// <response code="409">Ya existe un descuento con ese nombre</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(DiscountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDiscountCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedCommand = command with { DiscountId = id };
        var result = await _mediator.Send(updatedCommand);

        return Ok(result);
    }

    /// <summary>
    /// DELETE /api/descuentos/{id} - Elimina un descuento
    /// </summary>
    /// <param name="id">ID del descuento a eliminar</param>
    /// <response code="204">Descuento eliminado exitosamente</response>
    /// <response code="404">Descuento no encontrado</response>
    /// <response code="409">No se puede eliminar el descuento "Sin descuento"</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteDiscountCommand { DiscountId = id };
        await _mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// PATCH /api/descuentos/{id}/estado - Alterna el estado activo/inactivo
    /// </summary>
    /// <param name="id">ID del descuento</param>
    /// <response code="200">Estado alternado exitosamente</response>
    /// <response code="404">Descuento no encontrado</response>
    /// <response code="409">No se puede desactivar el descuento "Sin descuento"</response>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(DiscountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var command = new ToggleDiscountStatusCommand(id);
        var result  = await _mediator.Send(command);

        return Ok(result);
    }

    #endregion
}
