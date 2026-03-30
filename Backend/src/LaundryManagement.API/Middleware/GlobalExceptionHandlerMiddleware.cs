using System.Net;
using System.Text.Json;
using LaundryManagement.API.Models;
using LaundryManagement.Domain.Exceptions;
using Npgsql;

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

            case NpgsqlException npgsqlException:
                statusCode = HttpStatusCode.InternalServerError;
                errorResponse.Code = "SQL_ERROR";
                errorResponse.Message = GetPostgresErrorMessage(npgsqlException);
                _logger.LogError(exception, "PostgreSQL error occurred: {Message}", npgsqlException.Message);
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

    private string GetPostgresErrorMessage(NpgsqlException npgsqlException)
    {
        // PostgreSQL SQLSTATE codes: https://www.postgresql.org/docs/current/errcodes-appendix.html
        return npgsqlException.SqlState switch
        {
            "08000" or "08006" or "08001" or "08004" =>
                "Error de conexión con la base de datos. Verifique que el servidor esté disponible.",
            "57014" =>
                "Timeout al procesar la operación en la base de datos.",
            "23505" =>
                "Ya existe un registro con los datos proporcionados.",
            "23503" =>
                "No se puede eliminar el registro porque tiene datos relacionados.",
            "23502" =>
                "No se puede insertar valor nulo en una columna requerida.",
            _ => _environment.IsDevelopment()
                ? $"Error PostgreSQL ({npgsqlException.SqlState}): {npgsqlException.Message}"
                : "Error al procesar la operación en la base de datos"
        };
    }
}
