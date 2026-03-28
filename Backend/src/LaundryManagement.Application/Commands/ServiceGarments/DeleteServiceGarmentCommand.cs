using MediatR;

namespace LaundryManagement.Application.Commands.ServiceGarments;

/// <summary>
/// Command para eliminar un tipo de prenda
/// </summary>
public sealed record DeleteServiceGarmentCommand : IRequest<Unit>
{
    /// <summary>
    /// ID del tipo de prenda a eliminar
    /// </summary>
    public int ServiceGarmentId { get; init; }

    public DeleteServiceGarmentCommand(int serviceGarmentId)
    {
        ServiceGarmentId = serviceGarmentId;
    }
}
