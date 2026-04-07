using LaundryManagement.Domain.Aggregates.Services;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Mappers;
using LaundryManagement.Infrastructure.Persistence;
using LaundryManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Infrastructure.Repositories;

/// <summary>
/// Repositorio para el agregado ServicePure con mapeo explícito entre Domain e Infrastructure.
/// Esta es la implementación DDD PURA con separación total de responsabilidades.
/// </summary>
public class ServiceRepositoryPure : IServiceRepository
{
    private readonly LaundryDbContext _context;
    private readonly ILogger<ServiceRepositoryPure> _logger;

    public ServiceRepositoryPure(LaundryDbContext context, ILogger<ServiceRepositoryPure> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ServicePure?> GetByIdAsync(ServiceId serviceId, CancellationToken cancellationToken = default)
    {
        var servicioEntity = await _context.Servicios
            .Where(s => s.ServicioId == serviceId.Value)
            .Include(s => s.Categoria)
            .Include(s => s.ServiciosPrenda)
            .FirstOrDefaultAsync(cancellationToken);

        if (servicioEntity == null)
            return null;

        if (servicioEntity.Categoria == null) servicioEntity.Categoria = null!;
        if (servicioEntity.ServiciosPrenda == null) servicioEntity.ServiciosPrenda = new List<ServiciosPrenda>();

        return ServiceMapper.ToDomain(servicioEntity);
    }

    public async Task<ServicePure?> GetByCodeAsync(ServiceCode code, CancellationToken cancellationToken = default)
    {
        var servicioEntity = await _context.Servicios
            .Where(s => s.CodigoServicio == code.Value)
            .Include(s => s.Categoria)
            .Include(s => s.ServiciosPrenda)
            .FirstOrDefaultAsync(cancellationToken);

        if (servicioEntity == null)
            return null;

        if (servicioEntity.Categoria == null) servicioEntity.Categoria = null!;
        if (servicioEntity.ServiciosPrenda == null) servicioEntity.ServiciosPrenda = new List<ServiciosPrenda>();

        return ServiceMapper.ToDomain(servicioEntity);
    }

    public async Task<IEnumerable<ServicePure>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var servicioEntities = await _context.Servicios
            .Where(s => s.Activo)
            .Include(s => s.Categoria)
            .Include(s => s.ServiciosPrenda)
            .OrderBy(s => s.NombreServicio)
            .ToListAsync(cancellationToken);

        foreach (var entity in servicioEntities)
        {
            if (entity.Categoria == null) entity.Categoria = null!;
            if (entity.ServiciosPrenda == null) entity.ServiciosPrenda = new List<ServiciosPrenda>();
        }

        return servicioEntities.Select(ServiceMapper.ToDomain).ToList();
    }

    public async Task<IEnumerable<ServicePure>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var servicioEntities = await _context.Servicios
            .Include(s => s.Categoria)
            .Include(s => s.ServiciosPrenda)
            .OrderBy(s => s.NombreServicio)
            .ToListAsync(cancellationToken);

        foreach (var entity in servicioEntities)
        {
            if (entity.Categoria == null) entity.Categoria = null!;
            if (entity.ServiciosPrenda == null) entity.ServiciosPrenda = new List<ServiciosPrenda>();
        }

        return servicioEntities.Select(ServiceMapper.ToDomain).ToList();
    }

        public async Task<(int ServicioId, bool IsActive)?> GetServicePriceByIdAsync(int servicePriceId, CancellationToken cancellationToken = default)
    {
        var servicioPrenda = await _context.ServiciosPrendas
            .Where(sp => sp.ServicioPrendaId == servicePriceId)
            .Select(sp => new { sp.ServicioId, sp.Activo })
            .FirstOrDefaultAsync(cancellationToken);

        if (servicioPrenda == null)
            return null;

        return (servicioPrenda.ServicioId, servicioPrenda.Activo);
    }

    public async Task<Dictionary<int, ServicePure>> GetByIdsAsync(IEnumerable<int> serviceIds, CancellationToken cancellationToken = default)
    {
        var idList = serviceIds.Distinct().ToList();
        if (!idList.Any())
            return new Dictionary<int, ServicePure>();

        var entities = await _context.Servicios
            .Where(s => idList.Contains(s.ServicioId))
            .Include(s => s.Categoria)
            .Include(s => s.ServiciosPrenda)
            .ToListAsync(cancellationToken);

        // Proteger contra navegación null en mapeo: si Categoria falta, crear referencia vacía
        foreach (var entity in entities)
        {
            if (entity.Categoria == null)
            {
                entity.Categoria = null!;
            }
            if (entity.ServiciosPrenda == null)
            {
                entity.ServiciosPrenda = new List<ServiciosPrenda>();
            }
        }

        return entities.ToDictionary(e => e.ServicioId, ServiceMapper.ToDomain);
    }

    public async Task<ServicePure> AddAsync(ServicePure service, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando creación de servicio {ServiceCode}", service.Code.Value);

            // Mapeo explícito: Domain → Infrastructure
            var servicioEntity = ServiceMapper.ToInfrastructure(service);

            // Guardar los precios temporalmente
            var precios = servicioEntity.ServiciosPrenda.ToList();
            _logger.LogDebug("Servicio tiene {PreciosCount} precios", precios.Count);

            // Limpiar la colección para guardar primero solo el servicio
            servicioEntity.ServiciosPrenda.Clear();

            // Persistir el servicio primero para obtener el ServicioId generado
            _context.Servicios.Add(servicioEntity);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Servicio guardado exitosamente con ID: {ServicioId}", servicioEntity.ServicioId);

            // Actualizar el ID en el agregado de dominio
            service.SetId(ServiceId.From(servicioEntity.ServicioId));

            // Ahora asignar el ServicioId a los precios
            foreach (var precio in precios)
            {
                precio.ServicioId = servicioEntity.ServicioId;
                servicioEntity.ServiciosPrenda.Add(precio);
            }

            // Guardar los precios
            if (precios.Any())
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Precios guardados exitosamente");
            }

            // Retornar el servicio actualizado
            return service;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el servicio {ServiceCode}", service.Code.Value);
            throw;
        }
    }

    public async Task UpdateAsync(ServicePure service, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Actualizando servicio {ServiceId}", service.Id.Value);

            // Obtener la entidad existente con sus precios
            var existingEntity = await _context.Servicios
                .Include(s => s.ServiciosPrenda)
                .FirstOrDefaultAsync(s => s.ServicioId == service.Id.Value, cancellationToken);

            if (existingEntity == null)
            {
                throw new InvalidOperationException($"Servicio con ID {service.Id.Value} no encontrado");
            }

            // Mapear el servicio actualizado
            var updatedEntity = ServiceMapper.ToInfrastructure(service);

            // Actualizar propiedades del servicio
            existingEntity.CodigoServicio = updatedEntity.CodigoServicio;
            existingEntity.NombreServicio = updatedEntity.NombreServicio;
            existingEntity.Descripcion = updatedEntity.Descripcion;
            existingEntity.CategoriaId = updatedEntity.CategoriaId;
            existingEntity.TipoCobroServicio = updatedEntity.TipoCobroServicio;
            existingEntity.PrecioPorKilo = updatedEntity.PrecioPorKilo;
            existingEntity.PesoMinimo = updatedEntity.PesoMinimo;
            existingEntity.PesoMaximo = updatedEntity.PesoMaximo;
            existingEntity.Activo = updatedEntity.Activo;
            existingEntity.TiempoEstimado = updatedEntity.TiempoEstimado;

            // Actualizar precios (para servicios por pieza)
            // Eliminar precios que ya no existen
            var existingPriceIds = updatedEntity.ServiciosPrenda.Select(p => p.ServicioPrendaId).ToList();
            var pricesToRemove = existingEntity.ServiciosPrenda
                .Where(p => !existingPriceIds.Contains(p.ServicioPrendaId))
                .ToList();

            foreach (var price in pricesToRemove)
            {
                existingEntity.ServiciosPrenda.Remove(price);
            }

            // Agregar o actualizar precios
            foreach (var updatedPrice in updatedEntity.ServiciosPrenda)
            {
                if (updatedPrice.ServicioPrendaId == 0)
                {
                    // Precio nuevo (sin ID asignado aún) — siempre agregar
                    updatedPrice.ServicioId = existingEntity.ServicioId;
                    existingEntity.ServiciosPrenda.Add(updatedPrice);
                }
                else
                {
                    var existingPrice = existingEntity.ServiciosPrenda
                        .FirstOrDefault(p => p.ServicioPrendaId == updatedPrice.ServicioPrendaId);

                    if (existingPrice != null)
                    {
                        // Actualizar precio existente
                        existingPrice.PrecioUnitario = updatedPrice.PrecioUnitario;
                        existingPrice.Activo = updatedPrice.Activo;
                        existingPrice.FechaActualizacion = DateTime.UtcNow;
                    }
                    else
                    {
                        updatedPrice.ServicioId = existingEntity.ServicioId;
                        existingEntity.ServiciosPrenda.Add(updatedPrice);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Servicio actualizado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el servicio {ServiceId}", service.Id.Value);
            throw;
        }
    }

    public async Task DeleteAsync(ServiceId serviceId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Eliminando servicio {ServiceId}", serviceId.Value);

            var servicioEntity = await _context.Servicios
                .Include(s => s.ServiciosPrenda)
                .FirstOrDefaultAsync(s => s.ServicioId == serviceId.Value, cancellationToken);

            if (servicioEntity == null)
            {
                throw new InvalidOperationException($"Servicio con ID {serviceId.Value} no encontrado");
            }

            _context.Servicios.Remove(servicioEntity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Servicio eliminado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el servicio {ServiceId}", serviceId.Value);
            throw;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
