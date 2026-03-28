using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.DomainEvents.ServiceGarments;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Aggregates.ServiceGarments;

/// <summary>
/// Agregado ServiceGarmentPure - Representa un tipo de prenda (camisa, pantalón, etc.)
/// Es un aggregate root simple con mínima lógica de negocio (master data).
/// </summary>
public sealed class ServiceGarmentPure : AggregateRoot<ServiceGarmentId>
{
    #region Propiedades de Dominio

    /// <summary>
    /// Nombre del tipo de prenda
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Descripción del tipo de prenda
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Indica si el tipo de prenda está activo
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor privado para reconstitución desde BD
    /// </summary>
    private ServiceGarmentPure()
    {
        Name = string.Empty;
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Crea un nuevo tipo de prenda
    /// </summary>
    public static ServiceGarmentPure Create(
        string name,
        string? description = null)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("El nombre del tipo de prenda es requerido");

        var garment = new ServiceGarmentPure
        {
            Id = ServiceGarmentId.Empty(),
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        // Evento de dominio
        garment.RaiseDomainEvent(new ServiceGarmentCreated(
            0, // Se actualizará al persistir
            name,
            DateTime.UtcNow
        ));

        return garment;
    }

    /// <summary>
    /// Reconstituye un tipo de prenda desde la base de datos (usado por Repository)
    /// </summary>
    internal static ServiceGarmentPure Reconstitute(
        ServiceGarmentId id,
        string name,
        string? description,
        bool isActive,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        return new ServiceGarmentPure
        {
            Id = id,
            Name = name,
            Description = description,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    #endregion

    #region Métodos de Dominio

    /// <summary>
    /// Actualiza la información del tipo de prenda
    /// </summary>
    public void UpdateInfo(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("El nombre del tipo de prenda es requerido");

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ServiceGarmentUpdated(
            Id.Value,
            name,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Activa el tipo de prenda
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ServiceGarmentUpdated(Id.Value, Name, DateTime.UtcNow));
    }

    /// <summary>
    /// Desactiva el tipo de prenda
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ServiceGarmentUpdated(Id.Value, Name, DateTime.UtcNow));
    }

    /// <summary>
    /// Establece el ID (usado por el repository después de persistir)
    /// </summary>
    internal void SetId(ServiceGarmentId id)
    {
        if (!Id.IsEmpty)
            throw new InvalidOperationException("El ID ya ha sido establecido");

        Id = id;
    }

    #endregion
}
