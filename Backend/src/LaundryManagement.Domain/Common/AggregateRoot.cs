namespace LaundryManagement.Domain.Common;

/// <summary>
/// Clase base abstracta para todas las raíces de agregado del dominio.
/// Extiende Entity y agrega capacidades para manejar eventos de dominio.
/// </summary>
/// <typeparam name="TId">Tipo del identificador de la raíz del agregado</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>
    /// Colección de solo lectura de eventos de dominio pendientes
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Registra un evento de dominio para ser publicado
    /// </summary>
    /// <param name="domainEvent">Evento de dominio a registrar</param>
    protected void RaiseDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Limpia todos los eventos de dominio pendientes.
    /// Típicamente llamado después de que los eventos han sido publicados.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
