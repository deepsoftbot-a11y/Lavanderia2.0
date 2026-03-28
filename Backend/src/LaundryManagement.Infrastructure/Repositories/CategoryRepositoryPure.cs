using LaundryManagement.Domain.Aggregates.Categories;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Mappers;
using LaundryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Infrastructure.Repositories;

/// <summary>
/// Repositorio para el agregado CategoryPure con mapeo explícito entre Domain e Infrastructure.
/// </summary>
public class CategoryRepositoryPure : ICategoryRepository
{
    private readonly LaundryDbContext _context;
    private readonly ILogger<CategoryRepositoryPure> _logger;

    public CategoryRepositoryPure(LaundryDbContext context, ILogger<CategoryRepositoryPure> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CategoryPure?> GetByIdAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Categorias
            .FirstOrDefaultAsync(c => c.CategoriaId == categoryId.Value, cancellationToken);

        if (entity == null)
            return null;

        return CategoryMapper.ToDomain(entity);
    }

    public async Task<IEnumerable<CategoryPure>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.Categorias
            .Where(c => c.Activo)
            .OrderBy(c => c.NombreCategoria)
            .ToListAsync(cancellationToken);

        return entities.Select(CategoryMapper.ToDomain).ToList();
    }

    public async Task<IEnumerable<CategoryPure>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.Categorias
            .OrderBy(c => c.NombreCategoria)
            .ToListAsync(cancellationToken);

        return entities.Select(CategoryMapper.ToDomain).ToList();
    }

    public async Task<CategoryPure> AddAsync(CategoryPure category, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creando categoría {CategoryName}", category.Name);

            var entity = CategoryMapper.ToInfrastructure(category);

            _context.Categorias.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            category.SetId(CategoryId.From(entity.CategoriaId));

            _logger.LogInformation("Categoría creada con ID: {CategoryId}", entity.CategoriaId);

            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la categoría {CategoryName}", category.Name);
            throw;
        }
    }

    public async Task UpdateAsync(CategoryPure category, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Actualizando categoría {CategoryId}", category.Id.Value);

            var existingEntity = await _context.Categorias
                .FirstOrDefaultAsync(c => c.CategoriaId == category.Id.Value, cancellationToken);

            if (existingEntity == null)
            {
                throw new InvalidOperationException($"Categoría con ID {category.Id.Value} no encontrada");
            }

            existingEntity.NombreCategoria = category.Name;
            existingEntity.Descripcion = category.Description;
            existingEntity.Activo = category.IsActive;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Categoría actualizada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la categoría {CategoryId}", category.Id.Value);
            throw;
        }
    }

    public async Task DeleteAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Eliminando categoría {CategoryId}", categoryId.Value);

            var entity = await _context.Categorias
                .FirstOrDefaultAsync(c => c.CategoriaId == categoryId.Value, cancellationToken);

            if (entity == null)
            {
                throw new InvalidOperationException($"Categoría con ID {categoryId.Value} no encontrada");
            }

            _context.Categorias.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Categoría eliminada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la categoría {CategoryId}", categoryId.Value);
            throw;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
