namespace LaundryManagement.Application.DTOs.Clients;

/// <summary>
/// DTO que representa la información completa de un cliente.
/// Usado para respuestas de queries y lectura de datos.
/// </summary>
public sealed record ClientDto
{
    /// <summary>
    /// ID único del cliente
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Número de cliente único (formato: CLT-YYYYMMDD-NNNN)
    /// </summary>
    public string CustomerNumber { get; init; } = string.Empty;

    /// <summary>
    /// Nombre completo del cliente
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Número telefónico
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
    /// Límite de crédito
    /// </summary>
    public decimal CreditLimit { get; init; }

    /// <summary>
    /// Saldo actual
    /// </summary>
    public decimal CurrentBalance { get; init; }

    /// <summary>
    /// Indica si el cliente está activo
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Fecha de registro
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// ID del usuario que registró el cliente
    /// </summary>
    public int CreatedBy { get; init; }

    /// <summary>
    /// Indica si el cliente tiene crédito disponible (computed)
    /// </summary>
    public bool HasCreditAvailable { get; init; }

    /// <summary>
    /// Crédito disponible = CreditLimit - CurrentBalance (computed)
    /// </summary>
    public decimal AvailableCredit { get; init; }
}
