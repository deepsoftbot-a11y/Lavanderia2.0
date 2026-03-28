using System.Data;
using Dapper;
using LaundryManagement.Application.DTOs.Cortes;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Exceptions;
using Microsoft.Data.SqlClient;

namespace LaundryManagement.Infrastructure.Services;

public class CorteCajaService : ICorteCajaService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CorteCajaService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<RegistrarCorteCajaResponse> RegistrarCorteCajaAsync(RegistrarCorteCajaRequest request)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@CajeroID", request.CajeroID);
            parameters.Add("@FechaInicio", request.FechaInicio);
            parameters.Add("@FechaFin", request.FechaFin);
            parameters.Add("@TurnoDescripcion", request.TurnoDescripcion);
            parameters.Add("@TotalDeclaradoEfectivo", request.TotalDeclaradoEfectivo);
            parameters.Add("@TotalDeclaradoTarjeta", request.TotalDeclaradoTarjeta);
            parameters.Add("@TotalDeclaradoTransferencia", request.TotalDeclaradoTransferencia);
            parameters.Add("@TotalDeclaradoOtros", request.TotalDeclaradoOtros);
            parameters.Add("@Observaciones", request.Observaciones);
            parameters.Add("@CorteID", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "SP_RegistrarCorteCaja",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return new RegistrarCorteCajaResponse
            {
                CorteID = parameters.Get<int>("@CorteID")
            };
        }
        catch (SqlException ex)
        {
            throw new DatabaseException("Error al registrar el corte de caja en la base de datos", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al registrar el corte de caja", ex);
        }
    }

    public async Task AjustarCorteCajaAsync(AjustarCorteCajaRequest request)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@CorteID", request.CorteID);
            parameters.Add("@MontoAjuste", request.MontoAjuste);
            parameters.Add("@Motivo", request.Motivo);

            await connection.ExecuteAsync(
                "SP_AjustarCorteCaja",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        catch (SqlException ex)
        {
            throw new DatabaseException("Error al ajustar el corte de caja en la base de datos", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al ajustar el corte de caja", ex);
        }
    }

    public async Task<IEnumerable<HistorialCorteResponse>> ConsultarHistorialCortesAsync(ConsultarHistorialCortesRequest request)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@CajeroID", request.CajeroID);
            parameters.Add("@FechaInicio", request.FechaInicio);
            parameters.Add("@FechaFin", request.FechaFin);
            parameters.Add("@SoloConDiferencias", request.SoloConDiferencias);

            var result = await connection.QueryAsync<HistorialCorteResponse>(
                "SP_ConsultarHistorialCortes",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException("Error al consultar el historial de cortes en la base de datos", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al consultar el historial de cortes", ex);
        }
    }

    public async Task<DetalleCorteResponse> VerDetalleCorteAsync(int corteID)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@CorteID", corteID);

            var result = await connection.QueryFirstOrDefaultAsync<DetalleCorteResponse>(
                "SP_VerDetalleCorte",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result ?? new DetalleCorteResponse();
        }
        catch (SqlException ex)
        {
            throw new DatabaseException("Error al consultar el detalle del corte en la base de datos", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al consultar el detalle del corte", ex);
        }
    }
}
