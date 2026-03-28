using System.Net;
using System.Text.Json;
using LaundryManagement.API.Models;
using LaundryManagement.Domain.Exceptions;
using Microsoft.Data.SqlClient;

namespace LaundryManagement.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var errorResponse = new ErrorResponse
        {
            TraceId = context.TraceIdentifier
        };

        var statusCode = HttpStatusCode.InternalServerError;

        switch (exception)
        {
            case BaseException baseException:
                statusCode = (HttpStatusCode)baseException.StatusCode;
                errorResponse.Code = baseException.Code;
                errorResponse.Message = baseException.Message;

                if (baseException is ValidationException validationException)
                {
                    errorResponse.Errors = validationException.Errors;
                }

                _logger.LogWarning(exception, "Business exception occurred: {Message}", exception.Message);
                break;

            case SqlException sqlException:
                statusCode = HttpStatusCode.InternalServerError;
                errorResponse.Code = "SQL_ERROR";
                errorResponse.Message = GetSqlErrorMessage(sqlException);
                _logger.LogError(exception, "SQL error occurred: {Message}", sqlException.Message);
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                errorResponse.Code = "UNAUTHORIZED";
                errorResponse.Message = "No tiene permisos para realizar esta operación";
                _logger.LogWarning("Unauthorized access attempt");
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                errorResponse.Code = "INTERNAL_ERROR";
                errorResponse.Message = "Ha ocurrido un error interno en el servidor";
                _logger.LogError(exception, "Unhandled exception occurred");
                break;
        }

        // Include stack trace only in development
        if (_environment.IsDevelopment())
        {
            errorResponse.StackTrace = exception.StackTrace;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }

    private string GetSqlErrorMessage(SqlException sqlException)
    {
        return sqlException.Number switch
        {
            -1 => "Error de conexión con la base de datos. Verifique que el servidor esté disponible.",
            -2 => "Timeout al conectar con la base de datos.",
            2627 => "Ya existe un registro con los datos proporcionados.",
            547 => "No se puede eliminar el registro porque tiene datos relacionados.",
            2601 => "Violación de índice único.",
            515 => "No se puede insertar valor nulo en una columna requerida.",
            _ => _environment.IsDevelopment()
                ? $"Error SQL ({sqlException.Number}): {sqlException.Message}"
                : "Error al procesar la operación en la base de datos"
        };
    }
}
