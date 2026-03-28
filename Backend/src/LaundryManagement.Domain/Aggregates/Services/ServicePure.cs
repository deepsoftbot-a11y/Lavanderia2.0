using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.DomainEvents.Services;
using LaundryManagement.Domain.Entities;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Aggregates.Services;

/// <summary>
/// Agregado ServicePure - Entidad de dominio rica completamente independiente de infraestructura.
/// Representa un servicio de lavandería (lavado, planchado, tintorería, etc.) con sus reglas de negocio.
/// </summary>
public sealed class ServicePure : AggregateRoot<ServiceId>
{
    private readonly List<ServicePrice> _prices;

    #region Propiedades de Dominio

    /// <summary>
    /// Código único del servicio
    /// </summary>
    public ServiceCode Code { get; private set; }

    /// <summary>
    /// Nombre del servicio
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Descripción del servicio
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Referencia a la categoría del servicio
    /// </summary>
    public CategoryReference Category { get; private set; }

    /// <summary>
    /// ID de la categoría (acceso directo para compatibilidad)
    /// </summary>
    public CategoryId CategoryId => Category.Id;

    /// <summary>
    /// Tipo de unidad de cobro (PIECE o WEIGHT)
    /// </summary>
    public UnitType UnitType { get; private set; }

    /// <summary>
    /// Precio base (solo para servicios tipo PIECE)
    /// </summary>
    public Money? BasePrice { get; private set; }

    /// <summary>
    /// Información de precio por peso (solo para servicios tipo WEIGHT)
    /// </summary>
    public WeightPricing? WeightPricing { get; private set; }

    /// <summary>
    /// Indica si el servicio está activo
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Icono del servicio
    /// </summary>
    public string? Icon { get; private set; }

    /// <summary>
    /// Horas estimadas para completar el servicio
    /// </summary>
    public decimal? EstimatedHours { get; private set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Precios específicos por tipo de prenda (solo lectura)
    /// Solo aplica para servicios tipo PIECE
    /// </summary>
    public IReadOnlyCollection<ServicePrice> Prices => _prices.AsReadOnly();

    /// <summary>
    /// Verifica si el servicio es de cobro por pieza
    /// </summary>
    public bool IsPieceBased => UnitType.IsPiece;

    /// <summary>
    /// Verifica si el servicio es de cobro por peso
    /// </summary>
    public bool IsWeightBased => UnitType.IsWeight;

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor privado para reconstitución desde BD
    /// </summary>
    private ServicePure()
    {
        _prices = new List<ServicePrice>();
        Code = ServiceCode.From("DEFAULT");
        Name = string.Empty;
        Category = CategoryReference.Reconstitute(CategoryId.Empty(), null);
        UnitType = UnitType.Piece();
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Crea un nuevo servicio basado en cobro por pieza
    /// </summary>
    public static ServicePure CreatePieceBased(
        ServiceCode code,
        string name,
        CategoryReference category,
        Money? basePrice = null,
        string? description = null,
        string? icon = null,
        decimal? estimatedHours = null)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("El nombre del servicio es requerido");

        if (category.Id.IsEmpty)
            throw new ValidationException("La categoría es requerida");

        if (basePrice != null && (basePrice.IsZero || basePrice.Amount <= 0))
            throw new ValidationException("El precio base debe ser mayor que cero");

        if (estimatedHours.HasValue && estimatedHours.Value < 0)
            throw new ValidationException("Las horas estimadas no pueden ser negativas");

        var service = new ServicePure
        {
            Id = ServiceId.Empty(),
            Code = code,
            Name = name.Trim(),
            Description = description?.Trim(),
            Category = category,
            UnitType = UnitType.Piece(),
            BasePrice = basePrice,
            WeightPricing = null, // No aplica para PIECE
            IsActive = true,
            Icon = icon,
            EstimatedHours = estimatedHours,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        // Evento de dominio
        service.RaiseDomainEvent(new ServiceCreated(
            0, // Se actualizará al persistir
            code.Value,
            name,
            category.Id.Value,
            UnitType.Type.PIECE,
            DateTime.UtcNow
        ));

        return service;
    }

    /// <summary>
    /// Crea un nuevo servicio basado en cobro por peso
    /// </summary>
    public static ServicePure CreateWeightBased(
        ServiceCode code,
        string name,
        CategoryReference category,
        WeightPricing weightPricing,
        string? description = null,
        string? icon = null,
        decimal? estimatedHours = null)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("El nombre del servicio es requerido");

        if (category.Id.IsEmpty)
            throw new ValidationException("La categoría es requerida");

        if (estimatedHours.HasValue && estimatedHours.Value < 0)
            throw new ValidationException("Las horas estimadas no pueden ser negativas");

        var service = new ServicePure
        {
            Id = ServiceId.Empty(),
            Code = code,
            Name = name.Trim(),
            Description = description?.Trim(),
            Category = category,
            UnitType = UnitType.Weight(),
            BasePrice = null, // No aplica para WEIGHT
            WeightPricing = weightPricing,
            IsActive = true,
            Icon = icon,
            EstimatedHours = estimatedHours,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        // Evento de dominio
        service.RaiseDomainEvent(new ServiceCreated(
            0,
            code.Value,
            name,
            category.Id.Value,
            UnitType.Type.WEIGHT,
            DateTime.UtcNow
        ));

        return service;
    }

    /// <summary>
    /// Reconstituye un servicio desde la base de datos (usado por Repository)
    /// </summary>
    internal static ServicePure Reconstitute(
        ServiceId id,
        ServiceCode code,
        string name,
        string? description,
        CategoryReference category,
        UnitType unitType,
        Money? basePrice,
        WeightPricing? weightPricing,
        bool isActive,
        string? icon,
        decimal? estimatedHours,
        DateTime createdAt,
        DateTime? updatedAt,
        List<ServicePrice> prices)
    {
        var service = new ServicePure
        {
            Id = id,
            Code = code,
            Name = name,
            Description = description,
            Category = category,
            UnitType = unitType,
            BasePrice = basePrice,
            WeightPricing = weightPricing,
            IsActive = isActive,
            Icon = icon,
            EstimatedHours = estimatedHours,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        service._prices.AddRange(prices);

        return service;
    }

    #endregion

    #region Métodos de Dominio

    /// <summary>
    /// Actualiza la información básica del servicio
    /// </summary>
    public void UpdateInfo(
        string name,
        string? description = null,
        string? icon = null,
        decimal? estimatedHours = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("El nombre del servicio es requerido");

        if (estimatedHours.HasValue && estimatedHours.Value < 0)
            throw new ValidationException("Las horas estimadas no pueden ser negativas");

        Name = name.Trim();
        Description = description?.Trim();
        Icon = icon;
        EstimatedHours = estimatedHours;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ServiceUpdated(
            Id.Value,
            name,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Actualiza el precio base (solo para servicios PIECE)
    /// </summary>
    public void UpdateBasePrice(Money newBasePrice)
    {
        if (!IsPieceBased)
            throw new BusinessRuleException("Solo los servicios por pieza pueden tener precio base");

        if (newBasePrice.IsZero || newBasePrice.Amount <= 0)
            throw new ValidationException("El precio base debe ser mayor que cero");

        BasePrice = newBasePrice;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ServiceUpdated(Id.Value, Name, DateTime.UtcNow));
    }

    /// <summary>
    /// Actualiza el pricing por peso (solo para servicios WEIGHT)
    /// </summary>
    public void UpdateWeightPricing(WeightPricing newWeightPricing)
    {
        if (!IsWeightBased)
            throw new BusinessRuleException("Solo los servicios por peso pueden tener weight pricing");

        WeightPricing = newWeightPricing;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ServiceUpdated(Id.Value, Name, DateTime.UtcNow));
    }

    /// <summary>
    /// Agrega un precio específico para un tipo de prenda (solo para servicios PIECE)
    /// </summary>
    public void AddPriceForGarment(ServiceGarmentId garmentId, Money price)
    {
        if (!IsPieceBased)
            throw new BusinessRuleException("Solo los servicios por pieza pueden tener precios por prenda");

        // Verificar que no exista ya un precio activo para esta prenda
        if (_prices.Any(p => p.ServiceGarmentId == garmentId && p.IsActive))
            throw new BusinessRuleException($"Ya existe un precio activo para la prenda {garmentId.Value}");

        var servicePrice = ServicePrice.Create(Id, garmentId, price);
        _prices.Add(servicePrice);

        RaiseDomainEvent(new ServicePriceAdded(
            Id.Value,
            0, // Se actualizará al persistir
            garmentId.Value,
            price.Amount,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Actualiza el precio de una prenda específica
    /// </summary>
    public void UpdatePriceForGarment(ServiceGarmentId garmentId, Money newPrice)
    {
        if (!IsPieceBased)
            throw new BusinessRuleException("Solo los servicios por pieza pueden tener precios por prenda");

        var existingPrice = _prices.FirstOrDefault(p => p.ServiceGarmentId == garmentId && p.IsActive);
        if (existingPrice == null)
            throw new NotFoundException($"No existe un precio activo para la prenda {garmentId.Value}");

        existingPrice.UpdatePrice(newPrice);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ServicePriceUpdated(
            Id.Value,
            existingPrice.Id.Value,
            garmentId.Value,
            newPrice.Amount,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Obtiene el precio para un tipo de prenda específico
    /// </summary>
    public Money? GetPriceForGarment(ServiceGarmentId garmentId)
    {
        var price = _prices.FirstOrDefault(p => p.ServiceGarmentId == garmentId && p.IsActive);
        return price?.Price;
    }

    /// <summary>
    /// Activa el servicio
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ServiceStatusChanged(Id.Value, true, DateTime.UtcNow));
    }

    /// <summary>
    /// Desactiva el servicio
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ServiceStatusChanged(Id.Value, false, DateTime.UtcNow));
    }

    /// <summary>
    /// Actualiza el precio de una prenda por su ID de precio
    /// </summary>
    public void UpdatePriceById(ServicePriceId priceId, Money newPrice)
    {
        var price = _prices.FirstOrDefault(p => p.Id.Value == priceId.Value);
        if (price == null)
            throw new NotFoundException($"No existe precio con ID {priceId.Value}");

        price.UpdatePrice(newPrice);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Desactiva un precio por su ID (borrado lógico)
    /// </summary>
    public void DeactivatePriceById(ServicePriceId priceId)
    {
        var price = _prices.FirstOrDefault(p => p.Id.Value == priceId.Value);
        if (price == null)
            throw new NotFoundException($"No existe precio con ID {priceId.Value}");

        price.Deactivate();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Alterna el estado activo/inactivo de un precio por su ID
    /// </summary>
    public void TogglePriceStatusById(ServicePriceId priceId)
    {
        var price = _prices.FirstOrDefault(p => p.Id.Value == priceId.Value);
        if (price == null)
            throw new NotFoundException($"No existe precio con ID {priceId.Value}");

        if (price.IsActive) price.Deactivate();
        else price.Activate();

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Establece el ID (usado por el repository después de persistir)
    /// </summary>
    internal void SetId(ServiceId id)
    {
        if (!Id.IsEmpty)
            throw new InvalidOperationException("El ID ya ha sido establecido");

        Id = id;
    }

    #endregion
}
