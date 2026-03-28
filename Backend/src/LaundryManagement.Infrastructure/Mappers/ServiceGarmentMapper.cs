using LaundryManagement.Domain.Aggregates.ServiceGarments;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Persistence.Entities;

namespace LaundryManagement.Infrastructure.Mappers;

/// <summary>
/// Mapper que traduce entre entidades de dominio (ServiceGarmentPure) y entidades de infraestructura (TiposPrendum).
/// Esta es la capa de anti-corrupción que mantiene el dominio puro independiente de la base de datos.
/// </summary>
public static class ServiceGarmentMapper
{
    /// <summary>
    /// Mapea de entidad de dominio a entidad de infraestructura
    /// </summary>
    public static TiposPrendum ToInfrastructure(ServiceGarmentPure garment)
    {
        var entity = new TiposPrendum
        {
            NombrePrenda = garment.Name,
            Descripcion = garment.Description,
            Activo = garment.IsActive
        };

        // Solo asignar TipoPrendaId si ya existe (no es un nuevo tipo de prenda)
        if (!garment.Id.IsEmpty)
        {
            entity.TipoPrendaId = garment.Id.Value;
        }

        return entity;
    }

    /// <summary>
    /// Mapea de entidad de infraestructura a entidad de dominio
    /// </summary>
    public static ServiceGarmentPure ToDomain(TiposPrendum entity)
    {
        return ServiceGarmentPure.Reconstitute(
            id: ServiceGarmentId.From(entity.TipoPrendaId),
            name: entity.NombrePrenda,
            description: entity.Descripcion,
            isActive: entity.Activo,
            createdAt: DateTime.UtcNow, // TiposPrendum no tiene FechaCreacion, usar fecha actual
            updatedAt: null // TiposPrendum no tiene FechaActualizacion
        );
    }
}
