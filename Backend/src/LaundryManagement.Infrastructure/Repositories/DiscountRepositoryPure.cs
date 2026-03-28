using LaundryManagement.Domain.Aggregates.Discounts;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Mappers;
using LaundryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio para el agregado DiscountPure.
/// Usa EF Core para persistencia y DiscountMapper para traducción entre capas.
/// </summary>
public class DiscountRepositoryPure : IDiscountRepository
{
    private readonly LaundryDbContext _context;
    private readonly ILogger<DiscountRepositoryPure> _logger;

    public DiscountRepositoryPure(
        LaundryDbContext context,
        ILogger<DiscountRepositoryPure> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Queries (Lectura)

    public async Task<DiscountPure?> GetByIdAsync(DiscountId id, CancellationToken ct = default)
    {
        var entity = await _context.Descuentos
            .FirstOrDefaultAsync(d => d.DescuentoId == id.Value, ct);

        return entity == null ? null : DiscountMapper.ToDomain(entity);
    }

    public async Task<IEnumerable<DiscountPure>> GetAllAsync(CancellationToken ct = default)
    {
        var entities = await _context.Descuentos.ToListAsync(ct);
        return entities.Select(DiscountMapper.ToDomain).ToList();
    }

    public async Task<bool> ExistsByNameAsync(string name, DiscountId? excludeId = null, CancellationToken ct = default)
    {
        var query = _context.Descuentos.Where(d => d.NombreDescuento == name);

        if (excludeId != null)
            query = query.Where(d => d.DescuentoId != excludeId.Value);

        return await query.AnyAsync(ct);
    }

    #endregion

    #region Commands (Escritura)

    public async Task<DiscountPure> AddAsync(DiscountPure discount, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating discount: {Name}", discount.Name);

            var entity = DiscountMapper.ToInfrastructure(discount);
            _context.Descuentos.Add(entity);
            await _context.SaveChangesAsync(ct);

            // Asignar ID generado por BD al agregado
            discount.SetId(DiscountId.From(entity.DescuentoId));

            _logger.LogInformation("Discount created: ID={Id}", entity.DescuentoId);

            return discount;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating discount: {Name}", discount.Name);
            throw new DatabaseException(
                $"Error al crear el descuento en la base de datos: {ex.Message}", ex);
        }
    }

    public async Task UpdateAsync(DiscountPure discount, CancellationToken ct = default)
    {
        try
        {
            var existingEntity = await _context.Descuentos
                .FirstOrDefaultAsync(d => d.DescuentoId == discount.Id.Value, ct);

            if (existingEntity == null)
                throw new NotFoundException($"Descuento con ID {discount.Id.Value} no encontrado");

            _logger.LogInformation("Updating discount: ID={Id}", discount.Id.Value);

            // Actualizar propiedades desde el agregado de dominio
            existingEntity.NombreDescuento = discount.Name;
            existingEntity.TipoDescuento   = discount.Type.ToString();
            existingEntity.Valor           = discount.Value.Amount;
            existingEntity.Activo          = discount.IsActive;
            existingEntity.FechaInicio     = discount.ValidFrom;
            existingEntity.FechaFin        = discount.ValidUntil;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Discount updated: ID={Id}", discount.Id.Value);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating discount: ID={Id}", discount.Id.Value);
            throw new DatabaseException(
                $"Error al actualizar el descuento en la base de datos: {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            _logger.LogError(ex, "Unexpected error updating discount: ID={Id}", discount.Id.Value);
            throw;
        }
    }

    public async Task DeleteAsync(DiscountId id, CancellationToken ct = default)
    {
        var entity = await _context.Descuentos
            .FirstOrDefaultAsync(d => d.DescuentoId == id.Value, ct);

        if (entity == null)
            throw new NotFoundException($"Descuento con ID {id.Value} no encontrado");

        _context.Descuentos.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    #endregion
}
