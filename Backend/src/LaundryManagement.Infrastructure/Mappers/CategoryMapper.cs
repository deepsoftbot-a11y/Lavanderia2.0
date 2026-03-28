using LaundryManagement.Domain.Aggregates.Categories;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Persistence.Entities;

namespace LaundryManagement.Infrastructure.Mappers;

/// <summary>
/// Mapper que traduce entre entidades de dominio (CategoryPure) y entidades de infraestructura (Categoria).
/// Esta es la capa de anti-corrupción que mantiene el dominio puro independiente de la base de datos.
/// </summary>
public static class CategoryMapper
{
    /// <summary>
    /// Mapea de entidad de dominio a entidad de infraestructura
    /// </summary>
    public static Categoria ToInfrastructure(CategoryPure category)
    {
        var entity = new Categoria
        {
            NombreCategoria = category.Name,
            Descripcion = category.Description,
            Activo = category.IsActive
        };

        if (!category.Id.IsEmpty)
        {
            entity.CategoriaId = category.Id.Value;
        }

        return entity;
    }

    /// <summary>
    /// Mapea de entidad de infraestructura a entidad de dominio
    /// </summary>
    public static CategoryPure ToDomain(Categoria entity)
    {
        return CategoryPure.Reconstitute(
            id: CategoryId.From(entity.CategoriaId),
            name: entity.NombreCategoria,
            description: entity.Descripcion,
            isActive: entity.Activo
        );
    }
}
