using LaundryManagement.Domain.Aggregates.ServiceGarments;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Mappers;
using LaundryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Infrastructure.Repositories;

/// <summary>
/// Repositorio para el agregado ServiceGarmentPure con mapeo explícito entre Domain e Infrastructure.
/// Esta es la implementación DDD PURA con separación total de responsabilidades.
/// </summary>
public class ServiceGarmentRepositoryPure : IServiceGarmentRepository
{
    private readonly LaundryDbContext _context;
    private readonly ILogger<ServiceGarmentRepositoryPure> _logger;

    public ServiceGarmentRepositoryPure(LaundryDbContext context, ILogger<ServiceGarmentRepositoryPure> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ServiceGarmentPure?> GetByIdAsync(ServiceGarmentId garmentId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.TiposPrenda
            .FirstOrDefaultAsync(g => g.TipoPrendaId == garmentId.Value, cancellationToken);

        if (entity == null)
            return null;

        // Mapeo explícito: Infrastructure → Domain
        return ServiceGarmentMapper.ToDomain(entity);
    }

    public async Task<ServiceGarmentPure?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var entity = await _context.TiposPrenda
            .FirstOrDefaultAsync(g => g.NombrePrenda == name, cancellationToken);

        if (entity == null)
            return null;

        // Mapeo explícito: Infrastructure → Domain
        return ServiceGarmentMapper.ToDomain(entity);
    }

    public async Task<IEnumerable<ServiceGarmentPure>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.TiposPrenda
            .Where(g => g.Activo)
            .OrderBy(g => g.NombrePrenda)
            .ToListAsync(cancellationToken);

        // Mapeo explícito: Infrastructure → Domain
        return entities.Select(ServiceGarmentMapper.ToDomain).ToList();
    }

    public async Task<IEnumerable<ServiceGarmentPure>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.TiposPrenda
            .OrderBy(g => g.NombrePrenda)
            .ToListAsync(cancellationToken);

        // Mapeo explícito: Infrastructure → Domain
        return entities.Select(ServiceGarmentMapper.ToDomain).ToList();
    }

    public async Task<Dictionary<int, ServiceGarmentPure>> GetGarmentTypesByServicioPrendaIdsAsync(
        IEnumerable<int> servicioPrendaIds, CancellationToken cancellationToken = default)
    {
        var idList = servicioPrendaIds.Distinct().ToList();
        if (!idList.Any())
            return new Dictionary<int, ServiceGarmentPure>();

        var entities = await _context.ServiciosPrendas
            .Include(sp => sp.TipoPrenda)
            .Where(sp => idList.Contains(sp.ServicioPrendaId))
            .ToListAsync(cancellationToken);

        return entities.ToDictionary(
            sp => sp.ServicioPrendaId,
            sp => ServiceGarmentMapper.ToDomain(sp.TipoPrenda));
    }

    public async Task<ServiceGarmentPure> AddAsync(ServiceGarmentPure garment, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando creación de tipo de prenda {GarmentName}", garment.Name);

            // Mapeo explícito: Domain → Infrastructure
            var entity = ServiceGarmentMapper.ToInfrastructure(garment);

            // Persistir el tipo de prenda
            _context.TiposPrenda.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Tipo de prenda guardado exitosamente con ID: {GarmentId}", entity.TipoPrendaId);

            // Actualizar el ID en el agregado de dominio
            garment.SetId(ServiceGarmentId.From(entity.TipoPrendaId));

            // Retornar el tipo de prenda actualizado
            return garment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el tipo de prenda {GarmentName}", garment.Name);
            throw;
        }
    }

    public async Task UpdateAsync(ServiceGarmentPure garment, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Actualizando tipo de prenda {GarmentId}", garment.Id.Value);

            // Obtener la entidad existente
            var existingEntity = await _context.TiposPrenda
                .FirstOrDefaultAsync(g => g.TipoPrendaId == garment.Id.Value, cancellationToken);

            if (existingEntity == null)
            {
                throw new InvalidOperationException($"Tipo de prenda con ID {garment.Id.Value} no encontrado");
            }

            // Mapear el tipo de prenda actualizado
            var updatedEntity = ServiceGarmentMapper.ToInfrastructure(garment);

            // Actualizar propiedades
            existingEntity.NombrePrenda = updatedEntity.NombrePrenda;
            existingEntity.Descripcion = updatedEntity.Descripcion;
            existingEntity.Activo = updatedEntity.Activo;

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Tipo de prenda actualizado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el tipo de prenda {GarmentId}", garment.Id.Value);
            throw;
        }
    }

    public async Task DeleteAsync(ServiceGarmentId garmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Eliminando tipo de prenda {GarmentId}", garmentId.Value);

            var entity = await _context.TiposPrenda
                .FirstOrDefaultAsync(g => g.TipoPrendaId == garmentId.Value, cancellationToken);

            if (entity == null)
            {
                throw new InvalidOperationException($"Tipo de prenda con ID {garmentId.Value} no encontrado");
            }

            _context.TiposPrenda.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tipo de prenda eliminado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el tipo de prenda {GarmentId}", garmentId.Value);
            throw;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
