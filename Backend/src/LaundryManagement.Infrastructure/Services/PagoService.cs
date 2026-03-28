using System.Data;
using System.Text.Json;
using Dapper;
using LaundryManagement.Application.DTOs.Orders;
using LaundryManagement.Application.DTOs.Pagos;
using LaundryManagement.Application.DTOs.Payments;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Exceptions;
using Microsoft.Data.SqlClient;

namespace LaundryManagement.Infrastructure.Services;

public class PagoService : IPagoService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PagoService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<RegistrarPagoResponse> RegistrarPagoAsync(RegistrarPagoRequest request)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@OrdenID", request.OrdenID);
            parameters.Add("@MontoPago", request.MontoPago);
            parameters.Add("@RecibioPor", request.RecibioPor);
            parameters.Add("@MetodosPagoJSON", request.MetodosPagoJSON);
            parameters.Add("@Observaciones", request.Observaciones);
            parameters.Add("@PagoID", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "SP_RegistrarPago",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return new RegistrarPagoResponse
            {
                PagoID = parameters.Get<int>("@PagoID")
            };
        }
        catch (SqlException ex)
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

            var parameters = new DynamicParameters();
            parameters.Add("@ClienteID", clienteID);

            var result = await connection.QueryFirstOrDefaultAsync<ConsultarSaldoClienteResponse>(
                "SP_ConsultarSaldoCliente",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result ?? new ConsultarSaldoClienteResponse();
        }
        catch (SqlException ex)
        {
            throw new DatabaseException("Error al consultar el saldo del cliente en la base de datos", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al consultar el saldo del cliente", ex);
        }
    }

    public async Task<decimal> GetAmountPaidByOrderAsync(int orderId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<decimal?>(
                "SELECT ISNULL(SUM(MontoPago), 0) FROM Pagos WHERE OrdenId = @OrdenId AND CanceladoEn IS NULL",
                new { OrdenId = orderId }
            );
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
                "SELECT OrdenId, ISNULL(SUM(MontoPago), 0) AS Total FROM Pagos WHERE OrdenId IN @Ids AND CanceladoEn IS NULL GROUP BY OrdenId",
                new { Ids = ids }
            );
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

            // Trae pagos con el primer método de pago de cada pago (LEFT JOIN)
            const string sql = @"
                SELECT
                    p.PagoId       AS Id,
                    p.OrdenId      AS OrderId,
                    p.MontoPago    AS Amount,
                    ISNULL(pd.MetodoPagoId, 0) AS PaymentMethodId,
                    pd.Referencia  AS Reference,
                    p.Observaciones AS Notes,
                    p.FechaPago    AS PaidAt,
                    p.RecibioPor   AS ReceivedBy,
                    p.FechaPago    AS CreatedAt,
                    p.RecibioPor   AS CreatedBy
                FROM Pagos p
                LEFT JOIN (
                    SELECT PagoId, MIN(MetodoPagoId) AS MetodoPagoId, MIN(Referencia) AS Referencia
                    FROM PagosDetalle
                    GROUP BY PagoId
                ) pd ON pd.PagoId = p.PagoId
                WHERE p.OrdenId IN @Ids AND p.CanceladoEn IS NULL
                ORDER BY p.OrdenId, p.PagoId";

            var rows = await connection.QueryAsync<OrderPaymentDtoRaw>(sql, new { Ids = ids });

            return rows
                .GroupBy(r => r.OrderId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => new OrderPaymentDto
                    {
                        Id = r.Id,
                        OrderId = r.OrderId,
                        Amount = r.Amount,
                        PaymentMethodId = r.PaymentMethodId,
                        Reference = r.Reference,
                        Notes = r.Notes,
                        PaidAt = r.PaidAt.ToString("o"),
                        ReceivedBy = r.ReceivedBy,
                        CreatedAt = r.CreatedAt.ToString("o"),
                        CreatedBy = r.CreatedBy
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
            using var connection = _connectionFactory.CreateConnection();

            var methodsJson = JsonSerializer.Serialize(new[]
            {
                new
                {
                    MetodoPagoID = request.PaymentMethodId,
                    MontoPagado  = request.Amount,
                    Referencia   = request.Reference ?? string.Empty
                }
            });

            var parameters = new DynamicParameters();
            parameters.Add("@OrdenID",        request.OrderId);
            parameters.Add("@MontoPago",      request.Amount);
            parameters.Add("@RecibioPor",     request.ReceivedBy);
            parameters.Add("@MetodosPagoJSON", methodsJson);
            parameters.Add("@Observaciones",  request.Notes);
            parameters.Add("@PagoID", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "SP_RegistrarPago",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var pagoId = parameters.Get<int>("@PagoID");
            var created = await GetPaymentByIdAsync(pagoId);
            return created!;
        }
        catch (SqlException ex)
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

            const string sql = @"
                SELECT
                    p.PagoId        AS Id,
                    p.OrdenId       AS OrderId,
                    p.MontoPago     AS Amount,
                    ISNULL(pd.MetodoPagoId, 0) AS PaymentMethodId,
                    pd.Referencia   AS Reference,
                    p.Observaciones AS Notes,
                    p.FechaPago     AS PaidAt,
                    p.RecibioPor    AS ReceivedBy,
                    p.FechaPago     AS CreatedAt,
                    p.RecibioPor    AS CreatedBy
                FROM Pagos p
                LEFT JOIN (
                    SELECT PagoId, MIN(MetodoPagoId) AS MetodoPagoId, MIN(Referencia) AS Referencia
                    FROM PagosDetalle
                    GROUP BY PagoId
                ) pd ON pd.PagoId = p.PagoId
                WHERE p.PagoId = @PaymentId AND p.CanceladoEn IS NULL";

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
                "UPDATE Pagos SET CanceladoEn = GETDATE(), CanceladoPor = @CancelledBy WHERE PagoId = @PaymentId AND CanceladoEn IS NULL",
                new { PaymentId = paymentId, CancelledBy = cancelledBy }
            );

            if (affected == 0)
                throw new NotFoundException($"Pago {paymentId} no encontrado o ya cancelado");
        }
        catch (Exception ex) when (ex is not DatabaseException and not NotFoundException)
        {
            throw new DatabaseException($"Error al cancelar el pago {paymentId}", ex);
        }
    }

    // Tipo auxiliar para mapeo Dapper
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
