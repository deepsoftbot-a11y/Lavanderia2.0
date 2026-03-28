using MediatR;

namespace LaundryManagement.Application.Commands.ServiceGarments;

/// <summary>
/// Command para activar o desactivar un tipo de prenda
/// </summary>
public sealed record ToggleServiceGarmentStatusCommand : IRequest<Unit>
{
    /// <summary>
    /// ID del tipo de prenda
    /// </summary>
    public int ServiceGarmentId { get; init; }

    /// <summary>
    /// Estado deseado (true = activo, false = inactivo)
    /// </summary>
    public bool IsActive { get; init; }
}
