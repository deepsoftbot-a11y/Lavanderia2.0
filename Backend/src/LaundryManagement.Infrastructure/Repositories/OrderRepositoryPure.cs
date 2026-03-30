using System.Data.Common;
using System.Text.Json;
using Dapper;
using LaundryManagement.Domain.Aggregates.Orders;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Mappers;
using LaundryManagement.Infrastructure.Persistence;
using LaundryManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Infrastructure.Repositories;

/// <summary>
/// Repositorio para el agregado OrderPure con mapeo explícito entre Domain e Infrastructure.
/// Esta es la implementación DDD PURA con separación total de responsabilidades.
/// </summary>
public class OrderRepositoryPure : IOrderRepository
{
    private readonly LaundryDbContext _context;
    private readonly ILogger<OrderRepositoryPure> _logger;

    public OrderRepositoryPure(LaundryDbContext context, ILogger<OrderRepositoryPure> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OrderPure?> GetByIdAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        var ordenEntity = await _context.Ordenes
            .Include(o => o.OrdenesDetalles)
            .Include(o => o.OrdenesDescuentos)
            .FirstOrDefaultAsync(o => o.OrdenId == orderId.Value, cancellationToken);

        if (ordenEntity == null)
            return null;

        // Mapeo explícito: Infrastructure → Domain
        return OrderMapper.ToDomain(ordenEntity);
    }

    public async Task<OrderPure?> GetByFolioAsync(OrderFolio folio, CancellationToken cancellationToken = default)
    {
        var ordenEntity = await _context.Ordenes
            .Include(o => o.OrdenesDetalles)
            .Include(o => o.OrdenesDescuentos)
            .FirstOrDefaultAsync(o => o.FolioOrden == folio.Value, cancellationToken);

        if (ordenEntity == null)
            return null;

        // Mapeo explícito: Infrastructure → Domain
        return OrderMapper.ToDomain(ordenEntity);
    }

    public async Task<IEnumerable<OrderPure>> GetByClientAsync(ClientId clientId, CancellationToken cancellationToken = default)
    {
        var ordenEntities = await _context.Ordenes
            .Include(o => o.OrdenesDetalles)
            .Include(o => o.OrdenesDescuentos)
            .Where(o => o.ClienteId == clientId.Value)
            .ToListAsync(cancellationToken);

        // Mapeo explícito: Infrastructure → Domain
        return ordenEntities.Select(OrderMapper.ToDomain).ToList();
    }

    public async Task<OrderPure> AddAsync(OrderPure order, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando creación de orden para cliente {ClientId}", order.ClientId.Value);

            // Generar folio
            _logger.LogDebug("Generando folio...");
            var nextFolio = await GenerateNextFolioAsync(cancellationToken);
            order.SetFolio(OrderFolio.FromString(nextFolio));
            _logger.LogDebug("Folio generado: {Folio}", nextFolio);

            // Mapeo explícito: Domain → Infrastructure
            _logger.LogDebug("Mapeando dominio a infraestructura...");
            var ordenEntity = OrderMapper.ToInfrastructure(order);

            _logger.LogDebug("Orden tiene {DetallesCount} detalles y {DescuentosCount} descuentos",
                ordenEntity.OrdenesDetalles.Count, ordenEntity.OrdenesDescuentos.Count);

            // Agregar toda la jerarquía al contexto
            // EF Core manejará automáticamente el orden de inserción:
            // 1. Inserta Ordene (para obtener OrdenId)
            // 2. Actualiza OrdenId en detalles y descuentos en memoria (relationship fix-up)
            // 3. Inserta OrdenesDetalle y OrdenesDescuentos con el OrdenId correcto
            _logger.LogDebug("Agregando orden y toda su jerarquía al contexto...");
            _context.Ordenes.Add(ordenEntity);

            // Un solo SaveChanges para toda la jerarquía (orden + detalles + descuentos)
            _logger.LogDebug("Guardando orden completa en base de datos (transacción única)...");
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Orden guardada exitosamente con ID: {OrdenId}", ordenEntity.OrdenId);

            // Actualizar el ID en el dominio
            order.SetId(OrderId.From(ordenEntity.OrdenId));

            _logger.LogInformation("Orden creada completamente: OrdenId={OrdenId}, Folio={Folio}",
                ordenEntity.OrdenId, nextFolio);

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear orden para cliente {ClientId}: {Message}",
                order.ClientId.Value, ex.Message);
            throw;
        }
    }

    public async Task UpdateAsync(OrderPure order, CancellationToken cancellationToken = default)
    {
        var existingEntity = await _context.Ordenes
            .FirstOrDefaultAsync(o => o.OrdenId == order.Id.Value, cancellationToken);

        if (existingEntity == null)
            throw new InvalidOperationException($"No se encontró la orden con ID {order.Id.Value}");

        // Solo actualizar propiedades escalares — NO toca colecciones hijas
        existingEntity.EstadoOrdenId = order.StatusId;
        existingEntity.Subtotal = order.Subtotal.Amount;
        existingEntity.Descuento = order.TotalDiscount.Amount;
        existingEntity.Total = order.Total.Amount;
        existingEntity.FechaEntrega = order.DeliveryDate;
        existingEntity.EntregadoPor = order.DeliveredBy;
        existingEntity.Observaciones = order.Notes;
        existingEntity.Ubicaciones = order.StorageLocation;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceLineItemsAsync(OrderPure order, CancellationToken cancellationToken = default)
    {
        var existingEntity = await _context.Ordenes
            .Include(o => o.OrdenesDetalles)
            .Include(o => o.OrdenesDescuentos)
            .FirstOrDefaultAsync(o => o.OrdenId == order.Id.Value, cancellationToken);

        if (existingEntity == null)
            throw new InvalidOperationException($"No se encontró la orden con ID {order.Id.Value}");

        // Remover hijos del contexto antes de limpiar la colección
        _context.Set<Persistence.Entities.OrdenesDetalle>().RemoveRange(existingEntity.OrdenesDetalles);
        existingEntity.OrdenesDetalles.Clear();
        foreach (var lineItem in order.LineItems)
        {
            existingEntity.OrdenesDetalles.Add(new Persistence.Entities.OrdenesDetalle
            {
                OrdenDetalleId = lineItem.Id,
                NumeroLinea = lineItem.LineNumber,
                ServicioId = lineItem.ServiceId,
                ServicioPrendaId = lineItem.ServiceGarmentId,
                PesoKilos = lineItem.WeightKilos,
                Cantidad = lineItem.Quantity,
                PrecioUnitario = lineItem.UnitPrice.Amount,
                Subtotal = lineItem.Subtotal.Amount,
                DescuentoLinea = lineItem.LineDiscount.Amount,
                TotalLinea = lineItem.LineTotal.Amount,
                Observaciones = lineItem.Notes
            });
        }

        // Reemplazar descuentos
        _context.Set<Persistence.Entities.OrdenesDescuento>().RemoveRange(existingEntity.OrdenesDescuentos);
        existingEntity.OrdenesDescuentos.Clear();
        foreach (var discount in order.Discounts)
        {
            existingEntity.OrdenesDescuentos.Add(new Persistence.Entities.OrdenesDescuento
            {
                OrdenDescuentoId = discount.Id,
                DescuentoId = discount.DiscountId,
                ComboId = discount.ComboId,
                MontoDescuento = discount.DiscountAmount.Amount,
                Justificacion = discount.Justification,
                AplicadoPor = discount.AppliedBy
            });
        }

        // Actualizar también los escalares (totales recalculados)
        existingEntity.Subtotal = order.Subtotal.Amount;
        existingEntity.Descuento = order.TotalDiscount.Amount;
        existingEntity.Total = order.Total.Amount;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(OrderId orderId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Ordenes
            .Include(o => o.OrdenesDetalles)
            .Include(o => o.OrdenesDescuentos)
            .FirstOrDefaultAsync(o => o.OrdenId == orderId.Value, cancellationToken);

        if (entity != null)
        {
            _context.Ordenes.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Despachar eventos de dominio (implementar según necesidad)
        // await DispatchDomainEventsAsync(cancellationToken);

        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderPure>> GetAllAsync(
        string? search = null,
        int? clientId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string sortBy = "createdAt",
        string sortOrder = "desc",
        CancellationToken cancellationToken = default)
    {
        var query = _context.Ordenes
            .Include(o => o.OrdenesDetalles)
            .Include(o => o.OrdenesDescuentos)
            .Include(o => o.Cliente)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(o =>
                o.FolioOrden.ToLower().Contains(searchLower) ||
                o.Cliente.NombreCompleto.ToLower().Contains(searchLower) ||
                o.Cliente.Telefono.Contains(search));
        }

        if (clientId.HasValue)
            query = query.Where(o => o.ClienteId == clientId.Value);

        if (startDate.HasValue)
            query = query.Where(o => o.FechaRecepcion >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(o => o.FechaRecepcion <= endDate.Value);

        query = (sortBy.ToLower(), sortOrder.ToLower()) switch
        {
            ("folioorden", "asc")  => query.OrderBy(o => o.FolioOrden),
            ("folioorden", _)      => query.OrderByDescending(o => o.FolioOrden),
            ("total", "asc")       => query.OrderBy(o => o.Total),
            ("total", _)           => query.OrderByDescending(o => o.Total),
            (_, "asc")             => query.OrderBy(o => o.FechaRecepcion),
            _                      => query.OrderByDescending(o => o.FechaRecepcion)
        };

        var entities = await query.ToListAsync(cancellationToken);
        return entities.Select(OrderMapper.ToDomain).ToList();
    }

    public async Task<string> GenerateNextFolioAsync(CancellationToken cancellationToken)
    {
        string datePrefix = DateTime.Today.ToString("yyyyMMdd");
        string pattern = $"ORD-{datePrefix}-%";

        var lastOrder = await _context.Ordenes
            .Where(o => EF.Functions.Like(o.FolioOrden, pattern))
            .OrderByDescending(o => o.FolioOrden)
            .FirstOrDefaultAsync(cancellationToken);

        int nextNumber = 1;
        if (lastOrder != null)
        {
            string lastFolio = lastOrder.FolioOrden;
            if (lastFolio.Length >= 4)
            {
                string numberPart = lastFolio.Substring(lastFolio.Length - 4);
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
        }

        return $"ORD-{datePrefix}-{nextNumber:D4}";
    }

    public async Task<(OrderPure Order, int? PaymentId)> CreateOrderWithPaymentAsync(
        OrderPure order,
        string preGeneratedFolio,
        InitialPaymentData? paymentData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Iniciando creación atómica de orden y pago para cliente {ClientId} con folio {Folio}",
                order.ClientId.Value, preGeneratedFolio
            );

            // Asignar folio pre-generado
            order.SetFolio(OrderFolio.FromString(preGeneratedFolio));

            // Crear estrategia de ejecución compatible con retry logic
            var strategy = _context.Database.CreateExecutionStrategy();

            // Ejecutar toda la operación dentro de la estrategia de retry
            return await strategy.ExecuteAsync(async () =>
            {
                // Iniciar transacción explícita dentro de la estrategia
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Mapear y agregar orden al contexto
                    var ordenEntity = OrderMapper.ToInfrastructure(order);
                    _context.Ordenes.Add(ordenEntity);

                    _logger.LogDebug("Orden mapeada con {DetallesCount} detalles y {DescuentosCount} descuentos",
                        ordenEntity.OrdenesDetalles.Count, ordenEntity.OrdenesDescuentos.Count);

                    // Guardar orden con toda su jerarquía
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Orden guardada exitosamente: OrdenID={OrdenId}, Folio={Folio}",
                        ordenEntity.OrdenId, preGeneratedFolio);

                    // Actualizar ID en el dominio
                    order.SetId(OrderId.From(ordenEntity.OrdenId));

                    // Registrar pago si se proporciona
                    int? paymentId = null;
                    if (paymentData != null)
                    {
                        _logger.LogDebug("Registrando pago inicial para OrdenID={OrdenId}", ordenEntity.OrdenId);

                        // Generar folio del pago
                        var paymentFolio = await GeneratePaymentFolioAsync(cancellationToken);

                        // Insertar Pago
                        var pagoEntity = new Pago
                        {
                            FolioPago    = paymentFolio,
                            OrdenId      = ordenEntity.OrdenId,
                            MontoPago    = paymentData.Amount,
                            RecibioPor   = paymentData.ReceivedBy,
                            Observaciones = paymentData.Notes
                        };
                        _context.Pagos.Add(pagoEntity);
                        await _context.SaveChangesAsync(cancellationToken);

                        // Parsear métodos de pago e insertar PagosDetalle
                        var metodos = JsonSerializer.Deserialize<List<MetodoPagoJson>>(
                            paymentData.PaymentMethodsJson,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                            ?? new List<MetodoPagoJson>();

                        foreach (var m in metodos)
                        {
                            _context.PagosDetalles.Add(new PagosDetalle
                            {
                                PagoId       = pagoEntity.PagoId,
                                MetodoPagoId = m.MetodoPagoID,
                                MontoPagado  = m.MontoPagado,
                                Referencia   = m.Referencia ?? string.Empty
                            });
                        }
                        await _context.SaveChangesAsync(cancellationToken);

                        paymentId = pagoEntity.PagoId;

                        _logger.LogInformation(
                            "Pago registrado exitosamente: PagoID={PaymentId}, OrdenID={OrderId}, Monto={Amount}",
                            paymentId, ordenEntity.OrdenId, paymentData.Amount
                        );
                    }

                    // Confirmar transacción
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation(
                        "Orden y pago creados exitosamente en transacción atómica: OrdenID={OrderId}, PagoID={PaymentId}",
                        ordenEntity.OrdenId, paymentId
                    );

                    return (order, paymentId);
                }
                catch (Exception)
                {
                    // Rollback automático en caso de error
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al crear orden con pago para cliente {ClientId}: {Message}",
                order.ClientId.Value, ex.Message
            );
            throw;
        }
    }

    private async Task<string> GeneratePaymentFolioAsync(CancellationToken ct)
    {
        var datePrefix = DateTime.Today.ToString("yyyyMMdd");
        var pattern    = $"PAG-{datePrefix}-%";

        var lastFolio = await _context.Pagos
            .Where(p => EF.Functions.Like(p.FolioPago, pattern))
            .OrderByDescending(p => p.FolioPago)
            .Select(p => p.FolioPago)
            .FirstOrDefaultAsync(ct);

        int nextNumber = 1;
        if (lastFolio != null && lastFolio.Length >= 4 && int.TryParse(lastFolio[^4..], out int n))
            nextNumber = n + 1;

        return $"PAG-{datePrefix}-{nextNumber:D4}";
    }

    private sealed class MetodoPagoJson
    {
        public int     MetodoPagoID { get; set; }
        public decimal MontoPagado  { get; set; }
        public string? Referencia   { get; set; }
    }
}
