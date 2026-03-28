namespace LaundryManagement.Domain.Common;

/// <summary>
/// Clase base abstracta para todos los eventos de dominio.
/// Un evento de dominio representa algo que ha ocurrido en el dominio que es de interés para otros componentes.
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// Identificador único del evento
    /// </summary>
    public Guid EventId { get; }

    /// <summary>
    /// Fecha y hora en que ocurrió el evento
    /// </summary>
    public DateTime OccurredOn { get; }

    /// <summary>
    /// Constructor protegido para eventos de dominio
    /// </summary>
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}
