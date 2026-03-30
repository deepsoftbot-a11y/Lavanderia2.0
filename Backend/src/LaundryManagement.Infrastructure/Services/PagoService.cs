using System.Data.Common;
using System.Text.Json;
using Dapper;
using LaundryManagement.Application.DTOs.Orders;
using LaundryManagement.Application.DTOs.Pagos;
using LaundryManagement.Application.DTOs.Payments;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Infrastructure.Persistence;
using LaundryManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaundryManagement.Infrastructure.Services;

public class PagoService : IPagoService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly LaundryDbContext _context;

    public PagoService(IDbConnectionFactory connectionFactory, LaundryDbContext context)
    {
        _connectionFactory = connectionFactory;
        _context = context;
    }

    public async Task<RegistrarPagoResponse> RegistrarPagoAsync(RegistrarPagoRequest request)
    {
        try
        {
            var metodos = JsonSerializer.Deserialize<List<MetodoPagoJson>>(
                request.MetodosPagoJSON,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new Domain.Exceptions.ValidationException("Los métodos de pago no pueden estar vacíos");

            var folio = await GenerateNextFolioAsync();

            var strategy = _context.Database.CreateExecutionStrategy();
            var pagoId = await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var pago = new Pago
                {
                    FolioPago    = folio,
                    OrdenId      = request.OrdenID,
                    MontoPago    = request.MontoPago,
                    RecibioPor   = request.RecibioPor,
                    Observaciones = request.Observaciones
                };

                _context.Pagos.Add(pago);
                await _context.SaveChangesAsync();

                foreach (var m in metodos)
                {
                    _context.PagosDetalles.Add(new PagosDetalle
                    {
                        PagoId       = pago.PagoId,
                        MetodoPagoId = m.MetodoPagoID,
                        MontoPagado  = m.MontoPagado,
                        Referencia   = m.Referencia
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return pago.PagoId;
            });

            return new RegistrarPagoResponse { PagoID = pagoId };
        }
        catch (Domain.Exceptions.ValidationException) { throw; }
        catch (DbException ex)
        {
            throw new DatabaseException("Error al registrar el pago en la base de datos", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al registrar el pago", ex);
        }
    }

    public async Task<ConsultarSaldoClienteResponse> ConsultarSaldoClienteAsync(int clienteID)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT
                    c."ClienteID"    AS ClienteID,
                    c."NombreCompleto" AS NombreCliente,
                    COALESCE(SUM(o."Total"), 0)                                         AS TotalOrdenes,
                    COALESCE(SUM(p."TotalPagado"), 0)                                   AS TotalPagado,
                    COALESCE(SUM(o."Total") - SUM(COALESCE(p."TotalPagado", 0)), 0)     AS SaldoPendiente
                FROM "Clientes" c
                LEFT JOIN "Ordenes" o ON o."ClienteID" = c."ClienteID"
                    AND o."EstadoOrdenID" != 5
                LEFT JOIN (
                    SELECT "OrdenID", SUM("MontoPago") AS "TotalPagado"
                    FROM "Pagos"
                    WHERE "CanceladoEn" IS NULL
                    GROUP BY "OrdenID"
                ) p ON p."OrdenID" = o."OrdenID"
                WHERE c."ClienteID" = @ClienteID
                GROUP BY c."ClienteID", c."NombreCompleto"
                """;

            var result = await connection.QueryFirstOrDefaultAsync<ConsultarSaldoClienteResponse>(
                sql, new { ClienteID = clienteID });

            return result ?? new ConsultarSaldoClienteResponse { ClienteID = clienteID };
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error al consultar el saldo del cliente en la base de datos", ex);
        }
    }

    public async Task<decimal> GetAmountPaidByOrderAsync(int orderId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<decimal?>(
                @"SELECT COALESCE(SUM(""MontoPago""), 0) FROM ""Pagos"" WHERE ""OrdenID"" = @OrdenId AND ""CanceladoEn"" IS NULL",
                new { OrdenId = orderId });
            return result ?? 0m;
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException($"Error al consultar pagos de la orden {orderId}", ex);
        }
    }

    public async Task<Dictionary<int, decimal>> GetAmountsPaidByOrdersAsync(IEnumerable<int> orderIds)
    {
        var ids = orderIds.ToList();
        if (!ids.Any()) return new Dictionary<int, decimal>();

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rows = await connection.QueryAsync<(int OrdenId, decimal Total)>(
                @"SELECT ""OrdenID"" AS OrdenId, COALESCE(SUM(""MontoPago""), 0) AS Total
                  FROM ""Pagos""
                  WHERE ""OrdenID"" = ANY(@Ids) AND ""CanceladoEn"" IS NULL
                  GROUP BY ""OrdenID""",
                new { Ids = ids.ToArray() });
            return rows.ToDictionary(r => r.OrdenId, r => r.Total);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error al consultar totales de pago por órdenes", ex);
        }
    }

    public async Task<Dictionary<int, List<OrderPaymentDto>>> GetPaymentsByOrdersAsync(IEnumerable<int> orderIds)
    {
        var ids = orderIds.ToList();
        if (!ids.Any()) return new Dictionary<int, List<OrderPaymentDto>>();

        try
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT
                    p."PagoID"        AS Id,
                    p."OrdenID"       AS OrderId,
                    p."MontoPago"     AS Amount,
                    COALESCE(pd."MetodoPagoID", 0) AS PaymentMethodId,
                    pd."Referencia"   AS Reference,
                    p."Observaciones" AS Notes,
                    p."FechaPago"     AS PaidAt,
                    p."RecibioPor"    AS ReceivedBy,
                    p."FechaPago"     AS CreatedAt,
                    p."RecibioPor"    AS CreatedBy
                FROM "Pagos" p
                LEFT JOIN (
                    SELECT "PagoID", MIN("MetodoPagoID") AS "MetodoPagoID", MIN("Referencia") AS "Referencia"
                    FROM "PagosDetalle"
                    GROUP BY "PagoID"
                ) pd ON pd."PagoID" = p."PagoID"
                WHERE p."OrdenID" = ANY(@Ids) AND p."CanceladoEn" IS NULL
                ORDER BY p."OrdenID", p."PagoID"
                """;

            var rows = await connection.QueryAsync<OrderPaymentDtoRaw>(sql, new { Ids = ids.ToArray() });

            return rows
                .GroupBy(r => r.OrderId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => new OrderPaymentDto
                    {
                        Id              = r.Id,
                        OrderId         = r.OrderId,
                        Amount          = r.Amount,
                        PaymentMethodId = r.PaymentMethodId,
                        Reference       = r.Reference,
                        Notes           = r.Notes,
                        PaidAt          = r.PaidAt.ToString("o"),
                        ReceivedBy      = r.ReceivedBy,
                        CreatedAt       = r.CreatedAt.ToString("o"),
                        CreatedBy       = r.CreatedBy
                    }).ToList()
                );
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error al consultar pagos por órdenes", ex);
        }
    }

    public async Task<OrderPaymentDto> CreatePaymentAsync(CreatePaymentRequest request)
    {
        try
        {
            var folio = await GenerateNextFolioAsync();

            var strategy = _context.Database.CreateExecutionStrategy();
            var pagoId = await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var pago = new Pago
                {
                    FolioPago    = folio,
                    OrdenId      = request.OrderId,
                    MontoPago    = request.Amount,
                    RecibioPor   = request.ReceivedBy,
                    Observaciones = request.Notes
                };

                _context.Pagos.Add(pago);
                await _context.SaveChangesAsync();

                _context.PagosDetalles.Add(new PagosDetalle
                {
                    PagoId       = pago.PagoId,
                    MetodoPagoId = request.PaymentMethodId,
                    MontoPagado  = request.Amount,
                    Referencia   = request.Reference ?? string.Empty
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return pago.PagoId;
            });

            var created = await GetPaymentByIdAsync(pagoId);
            return created!;
        }
        catch (DbException ex)
        {
            throw new DatabaseException("Error al registrar el pago", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al registrar el pago", ex);
        }
    }

    public async Task<OrderPaymentDto?> GetPaymentByIdAsync(int paymentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT
                    p."PagoID"        AS Id,
                    p."OrdenID"       AS OrderId,
                    p."MontoPago"     AS Amount,
                    COALESCE(pd."MetodoPagoID", 0) AS PaymentMethodId,
                    pd."Referencia"   AS Reference,
                    p."Observaciones" AS Notes,
                    p."FechaPago"     AS PaidAt,
                    p."RecibioPor"    AS ReceivedBy,
                    p."FechaPago"     AS CreatedAt,
                    p."RecibioPor"    AS CreatedBy
                FROM "Pagos" p
                LEFT JOIN (
                    SELECT "PagoID", MIN("MetodoPagoID") AS "MetodoPagoID", MIN("Referencia") AS "Referencia"
                    FROM "PagosDetalle"
                    GROUP BY "PagoID"
                ) pd ON pd."PagoID" = p."PagoID"
                WHERE p."PagoID" = @PaymentId AND p."CanceladoEn" IS NULL
                """;

            var row = await connection.QueryFirstOrDefaultAsync<OrderPaymentDtoRaw>(sql, new { PaymentId = paymentId });
            if (row is null) return null;

            return new OrderPaymentDto
            {
                Id              = row.Id,
                OrderId         = row.OrderId,
                Amount          = row.Amount,
                PaymentMethodId = row.PaymentMethodId,
                Reference       = row.Reference,
                Notes           = row.Notes,
                PaidAt          = row.PaidAt.ToString("o"),
                ReceivedBy      = row.ReceivedBy,
                CreatedAt       = row.CreatedAt.ToString("o"),
                CreatedBy       = row.CreatedBy
            };
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException($"Error al consultar el pago {paymentId}", ex);
        }
    }

    public async Task<List<OrderPaymentDto>> GetPaymentsByOrderIdAsync(int orderId)
    {
        var dict = await GetPaymentsByOrdersAsync(new[] { orderId });
        return dict.TryGetValue(orderId, out var list) ? list : new List<OrderPaymentDto>();
    }

    public async Task CancelPaymentAsync(int paymentId, int cancelledBy)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var affected = await connection.ExecuteAsync(
                @"UPDATE ""Pagos"" SET ""CanceladoEn"" = NOW(), ""CanceladoPor"" = @CancelledBy
                  WHERE ""PagoID"" = @PaymentId AND ""CanceladoEn"" IS NULL",
                new { PaymentId = paymentId, CancelledBy = cancelledBy });

            if (affected == 0)
                throw new NotFoundException($"Pago {paymentId} no encontrado o ya cancelado");
        }
        catch (Exception ex) when (ex is not DatabaseException and not NotFoundException)
        {
            throw new DatabaseException($"Error al cancelar el pago {paymentId}", ex);
        }
    }

    private async Task<string> GenerateNextFolioAsync()
    {
        var datePrefix = DateTime.Today.ToString("yyyyMMdd");
        var pattern    = $"PAG-{datePrefix}-%";

        var lastPago = await _context.Pagos
            .Where(p => EF.Functions.Like(p.FolioPago, pattern))
            .OrderByDescending(p => p.FolioPago)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastPago != null)
        {
            var lastFolio = lastPago.FolioPago;
            if (lastFolio.Length >= 4 && int.TryParse(lastFolio[^4..], out int n))
                nextNumber = n + 1;
        }

        return $"PAG-{datePrefix}-{nextNumber:D4}";
    }

    private sealed class MetodoPagoJson
    {
        public int MetodoPagoID { get; set; }
        public decimal MontoPagado { get; set; }
        public string? Referencia { get; set; }
    }

    private sealed class OrderPaymentDtoRaw
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public int PaymentMethodId { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
        public DateTime PaidAt { get; set; }
        public int ReceivedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
    }
}
