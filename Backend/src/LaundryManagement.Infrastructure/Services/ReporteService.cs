using System.Data;
using Dapper;
using LaundryManagement.Application.DTOs.Reportes;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Exceptions;
using Microsoft.Data.SqlClient;

namespace LaundryManagement.Infrastructure.Services;

public class ReporteService : IReporteService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ReporteService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<VentaDiariaResponse> ReporteVentasDiariasAsync(DateTime fecha)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Fecha", fecha.Date);

            var result = await connection.QueryFirstOrDefaultAsync<VentaDiariaResponse>(
                "SP_ReporteVentasDiarias",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result ?? new VentaDiariaResponse { Fecha = fecha.Date };
        }
        catch (SqlException ex)
        {
            throw new DatabaseException("Error al generar el reporte de ventas diarias en la base de datos", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al generar el reporte de ventas diarias", ex);
        }
    }
}
