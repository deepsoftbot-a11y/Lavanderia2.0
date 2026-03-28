using LaundryManagement.Domain.Common;
using LaundryManagement.Domain.DomainEvents.Clients;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Domain.Aggregates.Clients;

/// <summary>
/// Agregado Client PURO - Entidad de dominio rica completamente independiente de infraestructura.
/// NO conoce EF Core, NO conoce base de datos, solo lógica de negocio.
/// </summary>
public sealed class ClientPure : AggregateRoot<ClientId>
{
    #region Propiedades de Dominio

    /// <summary>
    /// Identificador único del cliente
    /// </summary>
    public new ClientId Id { get; private set; } = null!;

    /// <summary>
    /// Número de cliente único (generado por trigger de BD)
    /// Formato: CLT-YYYYMMDD-NNNN
    /// </summary>
    public string CustomerNumber { get; private set; }

    /// <summary>
    /// Nombre completo del cliente
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Número telefónico (único)
    /// </summary>
    public PhoneNumber PhoneNumber { get; private set; }

    /// <summary>
    /// Email opcional
    /// </summary>
    public Email? Email { get; private set; }

    /// <summary>
    /// Dirección opcional
    /// </summary>
    public string? Address { get; private set; }

    /// <summary>
    /// RFC opcional (Registro Federal de Contribuyentes)
    /// </summary>
    public RFC? Rfc { get; private set; }

    /// <summary>
    /// Límite de crédito
    /// </summary>
    public Money CreditLimit { get; private set; }

    /// <summary>
    /// Saldo actual (READ-ONLY - solo actualizado por sistema de pagos)
    /// </summary>
    public Money CurrentBalance { get; private set; }

    /// <summary>
    /// Indica si el cliente está activo
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Fecha de registro
    /// </summary>
    public DateTime RegisteredAt { get; private set; }

    /// <summary>
    /// Usuario que registró el cliente
    /// </summary>
    public int RegisteredBy { get; private set; }

    /// <summary>
    /// Indica si el cliente tiene crédito disponible
    /// </summary>
    public bool HasCreditAvailable => CurrentBalance.IsLessThan(CreditLimit);

    /// <summary>
    /// Crédito disponible (CreditLimit - CurrentBalance)
    /// </summary>
    public Money AvailableCredit => CreditLimit - CurrentBalance;

    /// <summary>
    /// Indica si el cliente puede realizar pedidos
    /// </summary>
    public bool CanPlaceOrder => IsActive && HasCreditAvailable;

    #endregion

    #region Constructores

    /// <summary>
    /// Constructor privado para reconstitución desde BD
    /// </summary>
    private ClientPure()
    {
        CustomerNumber = string.Empty;
        Name = string.Empty;
        PhoneNumber = null!;
        CreditLimit = Money.Zero();
        CurrentBalance = Money.Zero();
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Crea un nuevo cliente
    /// </summary>
    /// <param name="name">Nombre completo</param>
    /// <param name="phoneNumber">Número telefónico</param>
    /// <param name="registeredBy">ID del usuario que registra</param>
    /// <param name="email">Email opcional</param>
    /// <param name="address">Dirección opcional</param>
    /// <param name="rfc">RFC opcional</param>
    /// <param name="creditLimit">Límite de crédito opcional (default: 0)</param>
    /// <returns>Cliente nuevo</returns>
    public static ClientPure Create(
        string name,
        PhoneNumber phoneNumber,
        int registeredBy,
        Email? email = null,
        string? address = null,
        RFC? rfc = null,
        Money? creditLimit = null)
    {
        // Validaciones de negocio
        ValidateName(name);
        ValidateRegisteredBy(registeredBy);

        var client = new ClientPure
        {
            CustomerNumber = string.Empty, // Generado por trigger de BD
            Name = name.Trim(),
            PhoneNumber = phoneNumber,
            Email = email,
            Address = address?.Trim(),
            Rfc = rfc,
            CreditLimit = creditLimit ?? Money.Zero(),
            CurrentBalance = Money.Zero(),
            IsActive = true,
            RegisteredAt = DateTime.Now,
            RegisteredBy = registeredBy
        };

        // Evento de dominio
        client.RaiseDomainEvent(new ClientCreated(
            0, // Se actualizará al persistir
            phoneNumber.Value,
            registeredBy,
            DateTime.Now
        ));

        return client;
    }

    /// <summary>
    /// Reconstituye un cliente desde la base de datos (usado por Repository)
    /// INTERNAL - Solo accesible desde Infrastructure via InternalsVisibleTo
    /// </summary>
    internal static ClientPure Reconstitute(
        ClientId id,
        string customerNumber,
        string name,
        PhoneNumber phoneNumber,
        Email? email,
        string? address,
        RFC? rfc,
        Money creditLimit,
        Money currentBalance,
        bool isActive,
        DateTime registeredAt,
        int registeredBy)
    {
        return new ClientPure
        {
            Id = id,
            CustomerNumber = customerNumber,
            Name = name,
            PhoneNumber = phoneNumber,
            Email = email,
            Address = address,
            Rfc = rfc,
            CreditLimit = creditLimit,
            CurrentBalance = currentBalance,
            IsActive = isActive,
            RegisteredAt = registeredAt,
            RegisteredBy = registeredBy
        };
    }

    #endregion

    #region Business Methods

    /// <summary>
    /// Actualiza la información del cliente (excluye balance)
    /// </summary>
    /// <param name="name">Nuevo nombre (opcional)</param>
    /// <param name="phoneNumber">Nuevo teléfono (opcional)</param>
    /// <param name="email">Nuevo email (opcional)</param>
    /// <param name="address">Nueva dirección (opcional)</param>
    /// <param name="rfc">Nuevo RFC (opcional)</param>
    /// <param name="creditLimit">Nuevo límite de crédito (opcional)</param>
    public void UpdateInformation(
        string? name = null,
        PhoneNumber? phoneNumber = null,
        Email? email = null,
        string? address = null,
        RFC? rfc = null,
        Money? creditLimit = null)
    {
        EnsureIsActive();

        bool hasChanges = false;

        // Actualizar nombre si se proporciona
        if (name != null && name != Name)
        {
            ValidateName(name);
            Name = name.Trim();
            hasChanges = true;
        }

        // Actualizar teléfono si se proporciona
        if (phoneNumber != null && !phoneNumber.Equals(PhoneNumber))
        {
            PhoneNumber = phoneNumber;
            hasChanges = true;
        }

        // Actualizar email (puede ser null para limpiar)
        if (email != Email)
        {
            Email = email;
            hasChanges = true;
        }

        // Actualizar dirección (puede ser null para limpiar)
        if (address != Address)
        {
            Address = address?.Trim();
            hasChanges = true;
        }

        // Actualizar RFC (puede ser null para limpiar)
        if (rfc != Rfc)
        {
            Rfc = rfc;
            hasChanges = true;
        }

        // Actualizar límite de crédito con validación de regla de negocio
        if (creditLimit != null && !creditLimit.Equals(CreditLimit))
        {
            // Regla de negocio: No se puede reducir el límite de crédito por debajo del saldo actual
            if (creditLimit.IsLessThan(CurrentBalance))
                throw new BusinessRuleException(
                    $"No se puede reducir el límite de crédito (${creditLimit.Amount:F2}) " +
                    $"por debajo del saldo actual (${CurrentBalance.Amount:F2})"
                );

            CreditLimit = creditLimit;
            hasChanges = true;
        }

        // Lanzar evento solo si hubo cambios
        if (hasChanges)
        {
            RaiseDomainEvent(new ClientUpdated(Id.Value, DateTime.Now));
        }
    }

    /// <summary>
    /// Activa el cliente
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            throw new BusinessRuleException("El cliente ya está activo");

        IsActive = true;
        RaiseDomainEvent(new ClientActivated(Id.Value, DateTime.Now));
    }

    /// <summary>
    /// Desactiva el cliente (soft delete)
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new BusinessRuleException("El cliente ya está inactivo");

        // Regla de negocio: No se puede desactivar con saldo pendiente
        if (!CurrentBalance.IsZero)
            throw new BusinessRuleException(
                $"No se puede desactivar un cliente con saldo pendiente (${CurrentBalance.Amount:F2}). " +
                "Debe liquidar el saldo antes de desactivar el cliente"
            );

        IsActive = false;
        RaiseDomainEvent(new ClientDeactivated(Id.Value, DateTime.Now));
    }

    /// <summary>
    /// Actualiza el saldo del cliente (llamado SOLO por repository después de operaciones de pago)
    /// INTERNAL - No expuesto a la capa de aplicación
    /// </summary>
    /// <param name="newBalance">Nuevo saldo</param>
    internal void UpdateBalance(Money newBalance)
    {
        if (newBalance.Amount < 0)
            throw new BusinessRuleException("El saldo no puede ser negativo");

        var previousBalance = CurrentBalance;
        CurrentBalance = newBalance;

        // Lanzar evento solo si hubo cambio
        if (!CurrentBalance.Equals(previousBalance))
        {
            RaiseDomainEvent(new ClientBalanceChanged(
                Id.Value,
                previousBalance.Amount,
                CurrentBalance.Amount,
                DateTime.Now
            ));
        }
    }

    /// <summary>
    /// Asigna el ID después de la persistencia (usado por repository)
    /// </summary>
    /// <param name="id">ID asignado por la base de datos</param>
    internal void SetId(ClientId id)
    {
        if (Id.Value > 0)
            throw new InvalidOperationException("El ID ya ha sido asignado");

        Id = id;
    }

    #endregion

    #region Private Validation Methods

    /// <summary>
    /// Valida que el cliente esté activo antes de permitir modificaciones
    /// </summary>
    private void EnsureIsActive()
    {
        if (!IsActive)
            throw new BusinessRuleException(
                "No se puede modificar un cliente inactivo. Active el cliente primero"
            );
    }

    /// <summary>
    /// Valida el nombre del cliente
    /// </summary>
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("El nombre del cliente no puede estar vacío");

        if (name.Trim().Length < 3)
            throw new ValidationException(
                "El nombre del cliente debe tener al menos 3 caracteres"
            );

        if (name.Length > 200)
            throw new ValidationException(
                "El nombre del cliente no puede exceder 200 caracteres"
            );
    }

    /// <summary>
    /// Valida el ID del usuario que registra
    /// </summary>
    private static void ValidateRegisteredBy(int registeredBy)
    {
        if (registeredBy <= 0)
            throw new ValidationException(
                "El ID del usuario que registra debe ser un valor positivo"
            );
    }

    #endregion
}
