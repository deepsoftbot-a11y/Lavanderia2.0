using MediatR;

namespace LaundryManagement.Application.Commands.Clients;

/// <summary>
/// Comando para actualizar un cliente existente
/// </summary>
public sealed record UpdateClientCommand : IRequest<UpdateClientResult>
{
    /// <summary>
    /// ID del cliente a actualizar
    /// </summary>
    public int ClientId { get; init; }

    /// <summary>
    /// Nuevo nombre (opcional)
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Nuevo teléfono (opcional)
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Nuevo email (opcional, null para limpiar)
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Nueva dirección (opcional, null para limpiar)
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Nuevo RFC (opcional, null para limpiar)
    /// </summary>
    public string? Rfc { get; init; }

    /// <summary>
    /// Nuevo límite de crédito (opcional)
    /// </summary>
    public decimal? CreditLimit { get; init; }

    // NOTE: CurrentBalance NO incluido (read-only)
    // El frontend puede enviarlo pero será ignorado
}

/// <summary>
/// Resultado de la actualización de un cliente
/// </summary>
public sealed record UpdateClientResult
{
    /// <summary>
    /// ID del cliente actualizado
    /// </summary>
    public int ClientId { get; init; }

    /// <summary>
    /// Número de cliente
    /// </summary>
    public string CustomerNumber { get; init; } = string.Empty;

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string Name { get; init; } = string.Empty;
}
