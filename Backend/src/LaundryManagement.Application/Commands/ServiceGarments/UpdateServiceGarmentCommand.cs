using MediatR;

namespace LaundryManagement.Application.Commands.ServiceGarments;

/// <summary>
/// Command para actualizar un tipo de prenda existente
/// </summary>
public sealed record UpdateServiceGarmentCommand : IRequest<Unit>
{
    /// <summary>
    /// ID del tipo de prenda a actualizar
    /// </summary>
    public int ServiceGarmentId { get; init; }

    /// <summary>
    /// Nuevo nombre del tipo de prenda
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Nueva descripción del tipo de prenda
    /// </summary>
    public string? Description { get; init; }
}
