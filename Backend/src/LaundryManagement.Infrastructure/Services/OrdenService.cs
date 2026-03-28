using System.Data;
using Dapper;
using LaundryManagement.Application.DTOs.Ordenes;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Exceptions;
using Microsoft.Data.SqlClient;

namespace LaundryManagement.Infrastructure.Services;

public class OrdenService : IOrdenService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrdenService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CrearOrdenResponse> CrearOrdenAsync(CrearOrdenRequest request)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@ClienteID", request.ClienteID);
            parameters.Add("@FechaPrometida", request.FechaPrometida);
            parameters.Add("@RecibidoPor", request.RecibidoPor);
            parameters.Add("@DetalleJSON", request.DetalleJSON);
            parameters.Add("@Observaciones", request.Observaciones);
            parameters.Add("@Ubicaciones", request.Ubicaciones);
            parameters.Add("@OrdenID", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "SP_CrearOrden",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return new CrearOrdenResponse
            {
                OrdenID = parameters.Get<int>("@OrdenID")
            };
        }
        catch (SqlException ex)
        {
            throw new DatabaseException("Error al crear la orden en la base de datos", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al crear la orden", ex);
        }
    }

    public async Task CambiarEstadoOrdenAsync(CambiarEstadoOrdenRequest request)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@OrdenID", request.OrdenID);
            parameters.Add("@NuevoEstadoID", request.NuevoEstadoID);
            parameters.Add("@CambiadoPor", request.CambiadoPor);
            parameters.Add("@Comentarios", request.Comentarios);

            await connection.ExecuteAsync(
                "SP_CambiarEstadoOrden",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        catch (SqlException ex)
        {
            throw new DatabaseException("Error al cambiar el estado de la orden en la base de datos", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al cambiar el estado de la orden", ex);
        }
    }
}
