using LaundryManagement.Domain.Aggregates.Discounts;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Persistence.Entities;

namespace LaundryManagement.Infrastructure.Mappers;

/// <summary>
/// Anti-corruption layer: traduce entre el agregado DiscountPure (Domain)
/// y la entidad Descuento (Infrastructure/EF Core).
/// </summary>
public static class DiscountMapper
{
    /// <summary>
    /// Entidad EF → Agregado de dominio (via Reconstitute)
    /// </summary>
    public static DiscountPure ToDomain(Descuento entity)
    {
        return DiscountPure.Reconstitute(
            id:         DiscountId.From(entity.DescuentoId),
            name:       entity.NombreDescuento,
            type:       DiscountType.From(entity.TipoDescuento),
            value:      Money.FromDecimal(entity.Valor),
            isActive:   entity.Activo,
            validFrom:  entity.FechaInicio,
            validUntil: entity.FechaFin
        );
    }

    /// <summary>
    /// Agregado de dominio → Entidad EF
    /// </summary>
    public static Descuento ToInfrastructure(DiscountPure discount)
    {
        var entity = new Descuento
        {
            NombreDescuento = discount.Name,
            TipoDescuento   = discount.Type.ToString(),
            Valor           = discount.Value.Amount,
            Activo          = discount.IsActive,
            FechaInicio     = discount.ValidFrom,
            FechaFin        = discount.ValidUntil
        };

        if (!discount.Id.IsEmpty)
            entity.DescuentoId = discount.Id.Value;

        return entity;
    }
}
