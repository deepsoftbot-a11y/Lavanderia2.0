using MediatR;

namespace LaundryManagement.Application.Commands.Clients;

/// <summary>
/// Comando para crear un nuevo cliente
/// </summary>
public sealed record CreateClientCommand : IRequest<CreateClientResult>
{
    /// <summary>
    /// Nombre completo del cliente
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Número telefónico (requerido, único)
    /// </summary>
    public string Phone { get; init; } = string.Empty;

    /// <summary>
    /// Email (opcional)
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Dirección (opcional)
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// RFC - Registro Federal de Contribuyentes (opcional)
    /// </summary>
    public string? Rfc { get; init; }

    /// <summary>
    /// Límite de crédito (opcional, default: 0)
    /// </summary>
    public decimal? CreditLimit { get; init; }

    // NOTE: RegisteredBy NO viene del frontend
    // Se obtiene del JWT token (por ahora hardcoded = 1)
}

/// <summary>
/// Resultado de la creación de un cliente
/// </summary>
public sealed record CreateClientResult
{
    /// <summary>
    /// ID del cliente creado
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Número de cliente generado
    /// </summary>
    public string CustomerNumber { get; init; } = string.Empty;

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Número telefónico del cliente
    /// </summary>
    public string Phone { get; init; } = string.Empty;

    /// <summary>
    /// Email del cliente (opcional)
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Fecha de registro
    /// </summary>
    public DateTime RegisteredAt { get; init; }
}
