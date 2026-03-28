using LaundryManagement.Domain.Aggregates.Clients;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Persistence.Entities;

namespace LaundryManagement.Infrastructure.Mappers;

/// <summary>
/// Mapper que traduce entre entidades de dominio (ClientPure) y entidades de infraestructura (Cliente).
/// Esta es la capa de anti-corrupción que mantiene el dominio puro independiente de la base de datos.
/// </summary>
public static class ClientMapper
{
    /// <summary>
    /// Mapea de entidad de dominio a entidad de infraestructura
    /// </summary>
    /// <param name="client">Agregado de dominio</param>
    /// <returns>Entidad de EF Core</returns>
    public static Cliente ToInfrastructure(ClientPure client)
    {
        var entity = new Cliente
        {
            // NO asignar ClienteId si es 0 (nuevo cliente) - dejar que SQL Server genere el IDENTITY
            NumeroCliente = client.CustomerNumber, // Será sobrescrito por trigger si está vacío
            NombreCompleto = client.Name,
            Telefono = client.PhoneNumber.Value,
            Email = client.Email?.Value,
            Direccion = client.Address,
            Rfc = client.Rfc?.Value,
            LimiteCredito = client.CreditLimit.Amount,
            SaldoActual = client.CurrentBalance.Amount,
            Activo = client.IsActive,
            FechaRegistro = client.RegisteredAt,
            RegistradoPor = client.RegisteredBy
        };

        // Solo asignar ClienteId si ya existe (no es un nuevo cliente)
        if (client.Id != null && client.Id.Value > 0)
        {
            entity.ClienteId = client.Id.Value;
        }

        return entity;
    }

    /// <summary>
    /// Mapea de entidad de infraestructura a entidad de dominio
    /// </summary>
    /// <param name="entity">Entidad de EF Core</param>
    /// <returns>Agregado de dominio reconstituido</returns>
    public static ClientPure ToDomain(Cliente entity)
    {
        return ClientPure.Reconstitute(
            id: ClientId.From(entity.ClienteId),
            customerNumber: entity.NumeroCliente,
            name: entity.NombreCompleto,
            phoneNumber: PhoneNumber.From(entity.Telefono),
            email: string.IsNullOrWhiteSpace(entity.Email)
                ? null
                : Email.From(entity.Email),
            address: entity.Direccion,
            rfc: string.IsNullOrWhiteSpace(entity.Rfc)
                ? null
                : RFC.From(entity.Rfc),
            creditLimit: Money.FromDecimal(entity.LimiteCredito),
            currentBalance: Money.FromDecimal(entity.SaldoActual),
            isActive: entity.Activo,
            registeredAt: entity.FechaRegistro,
            registeredBy: entity.RegistradoPor
        );
    }
}
