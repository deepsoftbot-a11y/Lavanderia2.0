using Dapper;
using LaundryManagement.Domain.Aggregates.CashClosings;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Mappers;
using LaundryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace LaundryManagement.Infrastructure.Repositories;

/// <summary>
/// Repositorio para el agregado CashClosingPure con mapeo explícito entre Domain e Infrastructure.
/// Esta es la implementación DDD PURA con separación total de responsabilidades.
/// </summary>
public class CashClosingRepositoryPure : ICashClosingRepository
{
    private readonly LaundryDbContext _context;
    private readonly ILogger<CashClosingRepositoryPure> _logger;

    public CashClosingRepositoryPure(LaundryDbContext context, ILogger<CashClosingRepositoryPure> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CashClosingPure?> GetByIdAsync(CashClosingId id, CancellationToken cancellationToken = default)
    {
        var corteEntity = await _context.CortesCajas
            .Include(c => c.CortesCajaDetalles)
            .FirstOrDefaultAsync(c => c.CorteId == id.Value, cancellationToken);

        if (corteEntity == null)
            return null;

        // Mapeo explícito: Infrastructure → Domain
        return CashClosingMapper.ToDomain(corteEntity);
    }

    public async Task<CashClosingPure?> GetByFolioAsync(CashClosingFolio folio, CancellationToken cancellationToken = default)
    {
        var corteEntity = await _context.CortesCajas
            .Include(c => c.CortesCajaDetalles)
            .FirstOrDefaultAsync(c => c.FolioCorte == folio.Value, cancellationToken);

        if (corteEntity == null)
            return null;

        // Mapeo explícito: Infrastructure → Domain
        return CashClosingMapper.ToDomain(corteEntity);
    }

    public async Task<IEnumerable<CashClosingPure>> GetByCashierAndDateRangeAsync(
        int cashierId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var corteEntities = await _context.CortesCajas
            .Include(c => c.CortesCajaDetalles)
            .Where(c => c.CajeroId == cashierId
                && c.FechaCorte >= startDate
                && c.FechaCorte <= endDate)
            .OrderByDescending(c => c.FechaCorte)
            .ToListAsync(cancellationToken);

        // Mapeo explícito: Infrastructure → Domain
        return corteEntities.Select(CashClosingMapper.ToDomain).ToList();
    }

    /// <summary>
    /// Genera el siguiente folio secuencial para cortes de caja.
    /// Formato: CORTE-yyyyMMdd-NNNN (ej: CORTE-20260212-0001)
    /// </summary>
    private async Task<CashClosingFolio> GenerateNextFolioAsync(CancellationToken cancellationToken)
    {
        string datePrefix = DateTime.Today.ToString("yyyyMMdd");
        string pattern = $"CORTE-{datePrefix}-%";

        var lastCorte = await _context.CortesCajas
            .Where(c => EF.Functions.Like(c.FolioCorte, pattern))
            .OrderByDescending(c => c.FolioCorte)
            .FirstOrDefaultAsync(cancellationToken);

        int nextNumber = 1;
        if (lastCorte != null)
        {
            string numberPart = lastCorte.FolioCorte.Substring(lastCorte.FolioCorte.LastIndexOf('-') + 1);
            if (int.TryParse(numberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return CashClosingFolio.FromSequential(DateTime.Today, nextNumber);
    }

    public async Task<DayPaymentTotals> GetDayTotalsAsync(DateTime fecha, int cashierId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Obteniendo totales del día {Fecha} para cajero {CashierId}", fecha.Date, cashierId);

            var connection = _context.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@Fecha", fecha.Date);
            parameters.Add("@CajeroID", cashierId);

            // DTO temporal para mapear el resultado del SP
            var result = await connection.QueryFirstOrDefaultAsync<DayPaymentTotalsDto>(
                "SP_ObtenerTotalesDia",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );

            if (result == null)
            {
                _logger.LogDebug("No se encontraron pagos para el día {Fecha} y cajero {CashierId}", fecha.Date, cashierId);
                return DayPaymentTotals.Empty();
            }

            // Convertir DTO a Value Object de dominio
            return DayPaymentTotals.Create(
                Money.FromDecimal(result.TotalEfectivo),
                Money.FromDecimal(result.TotalTarjeta),
                Money.FromDecimal(result.TotalTransferencia),
                Money.FromDecimal(result.TotalOtros)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener totales del día {Fecha} para cajero {CashierId}", fecha.Date, cashierId);
            throw;
        }
    }

    // DTO privado para mapear resultado del stored procedure
    private class DayPaymentTotalsDto
    {
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalTransferencia { get; set; }
        public decimal TotalOtros { get; set; }
        public decimal TotalPagado { get; set; }
    }

    public async Task<CashClosingPure> AddAsync(CashClosingPure cashClosing, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Iniciando creación de corte de caja para cajero {CashierId} del {StartDate} al {EndDate} via EF Core",
                cashClosing.CashierId,
                cashClosing.StartDate,
                cashClosing.EndDate);

            // 1. Generar folio secuencial (formato: CORTE-yyyyMMdd-NNNN)
            var folio = await GenerateNextFolioAsync(cancellationToken);
            cashClosing.SetFolio(folio);
            _logger.LogDebug("Folio generado: {Folio}", folio.Value);

            // 2. Mapeo explícito: Domain → Infrastructure (incluye header + detalles)
            _logger.LogDebug("Mapeando dominio a infraestructura...");
            var corteEntity = CashClosingMapper.ToInfrastructure(cashClosing);

            _logger.LogDebug("Corte tiene {DetallesCount} detalles por método de pago",
                corteEntity.CortesCajaDetalles.Count);

            // 3. Agregar toda la jerarquía al contexto
            // EF Core manejará automáticamente el orden de inserción:
            // - Inserta CortesCaja (para obtener CorteId via IDENTITY)
            // - Actualiza CorteId en CortesCajaDetalle (relationship fix-up)
            // - Inserta CortesCajaDetalle con el CorteId correcto
            _logger.LogDebug("Agregando corte de caja y detalles al contexto...");
            _context.CortesCajas.Add(corteEntity);

            // 4. Un solo SaveChanges para toda la jerarquía (transacción única)
            _logger.LogDebug("Guardando corte de caja en base de datos...");
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Corte de caja guardado exitosamente con ID: {CorteId}", corteEntity.CorteId);

            // 5. Actualizar el ID en el dominio (asignado por IDENTITY)
            cashClosing.SetId(CashClosingId.From(corteEntity.CorteId));

            _logger.LogInformation(
                "Corte de caja creado completamente: ID={CorteId}, Folio={Folio}, TotalEsperado={Esperado}, TotalDeclarado={Declarado}",
                corteEntity.CorteId,
                folio.Value,
                corteEntity.TotalEsperado,
                corteEntity.TotalDeclarado);

            // 6. Retornar el agregado hidratado con ID y Folio
            return cashClosing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear corte de caja para cajero {CashierId}", cashClosing.CashierId);
            throw new DatabaseException($"Error al crear corte de caja: {ex.Message}", ex);
        }
    }

    public async Task UpdateAsync(CashClosingPure cashClosing, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Actualizando corte de caja {CorteId}", cashClosing.Id.Value);

            // Buscar la entidad existente
            var existingEntity = await _context.CortesCajas
                .FirstOrDefaultAsync(c => c.CorteId == cashClosing.Id.Value, cancellationToken);

            if (existingEntity == null)
            {
                throw new InvalidOperationException($"Corte de caja con ID {cashClosing.Id.Value} no encontrado");
            }

            // Actualizar propiedades desde el agregado de dominio
            var updatedEntity = CashClosingMapper.ToInfrastructure(cashClosing);

            // Copiar valores actualizados
            existingEntity.MontoAjuste = updatedEntity.MontoAjuste;
            existingEntity.MotivoAjuste = updatedEntity.MotivoAjuste;
            existingEntity.FechaAjuste = updatedEntity.FechaAjuste;
            existingEntity.DiferenciaFinal = updatedEntity.DiferenciaFinal;
            existingEntity.Observaciones = updatedEntity.Observaciones;

            _logger.LogInformation("Corte de caja {CorteId} actualizado exitosamente", cashClosing.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar corte de caja {CorteId}", cashClosing.Id.Value);
            throw;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
