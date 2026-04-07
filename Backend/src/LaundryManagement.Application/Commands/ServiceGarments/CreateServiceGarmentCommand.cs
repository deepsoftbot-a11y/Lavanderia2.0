using LaundryManagement.Application.DTOs.ServiceGarments;
using MediatR;

namespace LaundryManagement.Application.Commands.ServiceGarments;

/// <summary>
/// Command para crear un nuevo tipo de prenda usando DDD
/// </summary>
public sealed record CreateServiceGarmentCommand : IRequest<ServiceGarmentDto>
{
    /// <summary>
    /// Nombre del tipo de prenda
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Descripción del tipo de prenda
    /// </summary>
    public string? Description { get; init; }
}
