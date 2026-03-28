using LaundryManagement.Domain.Aggregates.Services;
using LaundryManagement.Domain.Entities;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Persistence.Entities;

namespace LaundryManagement.Infrastructure.Mappers;

/// <summary>
/// Mapper que traduce entre entidades de dominio (ServicePure) y entidades de infraestructura (Servicio).
/// Esta es la capa de anti-corrupción que mantiene el dominio puro independiente de la base de datos.
/// </summary>
public static class ServiceMapper
{
    /// <summary>
    /// Mapea de entidad de dominio a entidad de infraestructura
    /// </summary>
    public static Servicio ToInfrastructure(ServicePure service)
    {
        var servicioEntity = new Servicio
        {
            CodigoServicio = service.Code.Value,
            NombreServicio = service.Name,
            Descripcion = service.Description,
            CategoriaId = service.CategoryId.Value,
            TipoCobroServicio = service.UnitType.ToDatabaseValue(), // "PIEZA" o "KILO"
            Activo = service.IsActive,
            TiempoEstimado = service.EstimatedHours.HasValue ? (int?)Math.Round(service.EstimatedHours.Value * 60) : null, // Convertir horas a minutos
            FechaCreacion = service.CreatedAt,
            ServiciosPrenda = service.Prices.Select(ToInfrastructure).ToList()
        };

        // Mapear precios según el tipo de unidad
        if (service.IsPieceBased && service.BasePrice != null)
        {
            // Para servicios por pieza, no guardamos precio en nivel de servicio
            // Los precios se guardan en ServiciosPrenda
            servicioEntity.PrecioPorKilo = null;
            servicioEntity.PesoMinimo = null;
            servicioEntity.PesoMaximo = null;
        }
        else if (service.IsWeightBased && service.WeightPricing != null)
        {
            // Para servicios por peso, guardamos en nivel de servicio
            servicioEntity.PrecioPorKilo = service.WeightPricing.PricePerKilo.Amount;
            servicioEntity.PesoMinimo = service.WeightPricing.MinimumWeight;
            servicioEntity.PesoMaximo = service.WeightPricing.MaximumWeight;
        }

        // Solo asignar ServicioId si ya existe (no es un nuevo servicio)
        if (!service.Id.IsEmpty)
        {
            servicioEntity.ServicioId = service.Id.Value;
        }

        return servicioEntity;
    }

    /// <summary>
    /// Mapea de entidad de infraestructura a entidad de dominio
    /// </summary>
    public static ServicePure ToDomain(Servicio servicioEntity)
    {
        var unitType = UnitType.FromDatabaseValue(servicioEntity.TipoCobroServicio);
        var prices = servicioEntity.ServiciosPrenda
            .Select(ToDomain)
            .ToList();

        // Determinar BasePrice o WeightPricing según el tipo
        Money? basePrice = null;
        WeightPricing? weightPricing = null;

        if (unitType.IsPiece)
        {
            // Para servicios por pieza: basePrice se puede calcular del primer precio o dejar como promedio
            // Por ahora, lo dejamos como el primer precio si existe, o 0 si no hay precios
            basePrice = prices.Any()
                ? prices.First().Price
                : Money.Zero();
        }
        else if (unitType.IsWeight && servicioEntity.PrecioPorKilo.HasValue)
        {
            // Para servicios por peso: crear WeightPricing
            weightPricing = WeightPricing.Create(
                Money.FromDecimal(servicioEntity.PrecioPorKilo.Value),
                servicioEntity.PesoMinimo,
                servicioEntity.PesoMaximo
            );
        }

        // Crear referencia a categoría
        var categoryReference = CategoryReference.Reconstitute(
            CategoryId.From(servicioEntity.CategoriaId),
            servicioEntity.Categoria?.NombreCategoria
        );

        return ServicePure.Reconstitute(
            id: ServiceId.From(servicioEntity.ServicioId),
            code: ServiceCode.From(servicioEntity.CodigoServicio),
            name: servicioEntity.NombreServicio,
            description: servicioEntity.Descripcion,
            category: categoryReference,
            unitType: unitType,
            basePrice: basePrice,
            weightPricing: weightPricing,
            isActive: servicioEntity.Activo,
            icon: null, // Campo no existe en DB
            estimatedHours: servicioEntity.TiempoEstimado.HasValue ? servicioEntity.TiempoEstimado.Value / 60m : null, // Convertir minutos a horas
            createdAt: servicioEntity.FechaCreacion,
            updatedAt: null, // Campo no existe en DB
            prices: prices
        );
    }

    /// <summary>
    /// Mapea ServicePrice de dominio a ServiciosPrenda de infraestructura
    /// </summary>
    private static ServiciosPrenda ToInfrastructure(ServicePrice price)
    {
        var entity = new ServiciosPrenda
        {
            ServicioId = price.ServiceId.Value,
            TipoPrendaId = price.ServiceGarmentId.Value,
            PrecioUnitario = price.Price.Amount,
            Activo = price.IsActive,
            FechaActualizacion = price.UpdatedAt ?? price.CreatedAt
        };

        // Solo asignar ServicioPrendaId si ya existe
        if (!price.Id.IsEmpty)
        {
            entity.ServicioPrendaId = price.Id.Value;
        }

        return entity;
    }

    /// <summary>
    /// Mapea ServiciosPrenda de infraestructura a ServicePrice de dominio
    /// </summary>
    private static ServicePrice ToDomain(ServiciosPrenda entity)
    {
        return ServicePrice.Reconstitute(
            id: ServicePriceId.From(entity.ServicioPrendaId),
            serviceId: ServiceId.From(entity.ServicioId),
            serviceGarmentId: ServiceGarmentId.From(entity.TipoPrendaId),
            price: Money.FromDecimal(entity.PrecioUnitario),
            isActive: entity.Activo,
            createdAt: entity.FechaActualizacion, // Usar FechaActualizacion como createdAt
            updatedAt: entity.FechaActualizacion
        );
    }
}
