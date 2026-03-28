using MediatR;

namespace LaundryManagement.Application.Commands.Services;

public sealed record UpdateServicePriceCommand : IRequest<Unit>
{
    /// <summary>
    /// ID del precio (se asigna desde la URL en el controller)
    /// </summary>
    public int ServicePriceId { get; init; }

    /// <summary>
    /// Nuevo precio unitario (viene del body)
    /// </summary>
    public decimal UnitPrice { get; init; }
}
