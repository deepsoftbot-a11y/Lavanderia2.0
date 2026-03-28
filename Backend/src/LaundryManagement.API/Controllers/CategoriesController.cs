using LaundryManagement.Application.Commands.Categories;
using LaundryManagement.Application.DTOs.Categories;
using LaundryManagement.Application.Queries.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaundryManagement.API.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Obtiene todas las categorías</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Obtiene una categoría por su ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken);
        if (result == null)
            return NotFound(new { message = $"Categoría con ID {id} no encontrada" });
        return Ok(result);
    }

    /// <summary>Crea una nueva categoría</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Id }, result);
    }

    /// <summary>Actualiza una categoría (patch semántico: solo aplica los campos recibidos)</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryBody body, CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand
        {
            CategoryId = id,
            Name = body.Name,
            Description = body.Description,
            IsActive = body.IsActive
        };
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Elimina una categoría</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Alterna el estado activo/inactivo de una categoría</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleCategoryStatus(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ToggleCategoryStatusCommand(id), cancellationToken);
        return Ok(result);
    }
}

public sealed record UpdateCategoryBody(string? Name, string? Description, bool? IsActive);
