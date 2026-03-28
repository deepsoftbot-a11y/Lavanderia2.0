using LaundryManagement.Domain.Aggregates.Clients;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.Repositories;
using LaundryManagement.Domain.ValueObjects;
using LaundryManagement.Infrastructure.Mappers;
using LaundryManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LaundryManagement.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio para el agregado ClientPure.
/// Usa EF Core para persistencia y ClientMapper para traducción entre capas.
/// </summary>
public class ClientRepositoryPure : IClientRepository
{
    private readonly LaundryDbContext _context;
    private readonly ILogger<ClientRepositoryPure> _logger;

    public ClientRepositoryPure(
        LaundryDbContext context,
        ILogger<ClientRepositoryPure> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Queries (Lectura)

    public async Task<ClientPure?> GetByIdAsync(ClientId clientId, CancellationToken ct = default)
    {
        var entity = await _context.Clientes
            .FirstOrDefaultAsync(c => c.ClienteId == clientId.Value, ct);

        return entity == null ? null : ClientMapper.ToDomain(entity);
    }

    public async Task<ClientPure?> GetByPhoneNumberAsync(PhoneNumber phoneNumber, CancellationToken ct = default)
    {
        var entity = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Telefono == phoneNumber.Value, ct);

        return entity == null ? null : ClientMapper.ToDomain(entity);
    }

    public async Task<IEnumerable<ClientPure>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.Clientes.AsQueryable();

        if (!includeInactive)
            query = query.Where(c => c.Activo);

        var entities = await query.ToListAsync(ct);

        return entities.Select(ClientMapper.ToDomain).ToList();
    }

    public async Task<IEnumerable<ClientPure>> GetByIdsAsync(IEnumerable<ClientId> clientIds, CancellationToken ct = default)
    {
        var ids = clientIds.Select(c => c.Value).ToList();
        if (!ids.Any()) return Enumerable.Empty<ClientPure>();

        var entities = await _context.Clientes
            .Where(c => ids.Contains(c.ClienteId))
            .ToListAsync(ct);

        return entities.Select(ClientMapper.ToDomain).ToList();
    }

    public async Task<bool> ExistsByPhoneNumberAsync(
        PhoneNumber phoneNumber,
        ClientId? excludeClientId = null,
        CancellationToken ct = default)
    {
        var query = _context.Clientes.Where(c => c.Telefono == phoneNumber.Value);

        if (excludeClientId != null)
            query = query.Where(c => c.ClienteId != excludeClientId.Value);

        return await query.AnyAsync(ct);
    }

    #endregion

    #region Commands (Escritura)

    public async Task<ClientPure> AddAsync(ClientPure client, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation(
                "Creating client: {Name}, Phone: {Phone}",
                client.Name,
                client.PhoneNumber.Value
            );

            // 1. Generar NumeroCliente en la aplicación
            var customerNumber = await GenerateCustomerNumberAsync(ct);

            // 2. Mapear dominio → infraestructura
            var entity = ClientMapper.ToInfrastructure(client);
            entity.NumeroCliente = customerNumber;  // Asignar el número generado

            // 3. Agregar al contexto y persistir
            _context.Clientes.Add(entity);
            await _context.SaveChangesAsync(ct);

            // 4. Reconstituir el agregado con el número generado
            var savedClient = ClientMapper.ToDomain(entity);

            _logger.LogInformation(
                "Client created successfully: ID={ClientId}, CustomerNumber={CustomerNumber}",
                entity.ClienteId,
                entity.NumeroCliente
            );

            return savedClient;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Database error creating client: {Name}, Phone: {Phone}",
                client.Name,
                client.PhoneNumber.Value
            );

            throw new DatabaseException(
                $"Error al crear el cliente en la base de datos: {ex.Message}",
                ex
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error creating client: {Name}",
                client.Name
            );
            throw;
        }
    }

    /// <summary>
    /// Genera el siguiente NumeroCliente en formato CLT-YYYYMMDD-NNNN
    /// </summary>
    private async Task<string> GenerateCustomerNumberAsync(CancellationToken ct)
    {
        var today = DateTime.Now.ToString("yyyyMMdd");
        var prefix = $"CLT-{today}-";

        // Obtener el último número del día
        var lastNumber = await _context.Clientes
            .Where(c => c.NumeroCliente.StartsWith(prefix))
            .OrderByDescending(c => c.NumeroCliente)
            .Select(c => c.NumeroCliente)
            .FirstOrDefaultAsync(ct);

        int nextSequence = 1;
        if (lastNumber != null)
        {
            // Extraer el número secuencial (últimos 4 dígitos)
            var sequencePart = lastNumber.Substring(lastNumber.Length - 4);
            if (int.TryParse(sequencePart, out int currentSequence))
            {
                nextSequence = currentSequence + 1;
            }
        }

        return $"{prefix}{nextSequence:D4}";
    }

    public async Task UpdateAsync(ClientPure client, CancellationToken ct = default)
    {
        try
        {
            var existingEntity = await _context.Clientes
                .FirstOrDefaultAsync(c => c.ClienteId == client.Id.Value, ct);

            if (existingEntity == null)
                throw new NotFoundException($"Cliente con ID {client.Id.Value} no encontrado");

            _logger.LogInformation(
                "Updating client: ID={ClientId}, Name={Name}",
                client.Id.Value,
                client.Name
            );

            // Actualizar propiedades desde el dominio
            existingEntity.NombreCompleto = client.Name;
            existingEntity.Telefono = client.PhoneNumber.Value;
            existingEntity.Email = client.Email?.Value;
            existingEntity.Direccion = client.Address;
            existingEntity.Rfc = client.Rfc?.Value;
            existingEntity.LimiteCredito = client.CreditLimit.Amount;
            existingEntity.SaldoActual = client.CurrentBalance.Amount;
            existingEntity.Activo = client.IsActive;

            // IMPORTANTE: NumeroCliente, FechaRegistro y RegistradoPor son inmutables
            // No se actualizan después de la creación

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Client updated successfully: ID={ClientId}",
                client.Id.Value
            );
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Database error updating client: ID={ClientId}",
                client.Id.Value
            );

            throw new DatabaseException(
                $"Error al actualizar el cliente en la base de datos: {ex.Message}",
                ex
            );
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            _logger.LogError(
                ex,
                "Unexpected error updating client: ID={ClientId}",
                client.Id.Value
            );
            throw;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    #endregion
}
