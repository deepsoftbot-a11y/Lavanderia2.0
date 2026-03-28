using System.Data;
using System.Data.Common;
using LaundryManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace LaundryManagement.Infrastructure.Persistence;

/// <summary>
/// Implementación del patrón Unit of Work que envuelve el DbContext.
/// IMPORTANTE: Con EnableRetryOnFailure activo, NO se pueden usar transacciones explícitas.
/// Este UnitOfWork ahora actúa como un coordinador sin gestionar transacciones manuales.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly LaundryDbContext _context;

    public UnitOfWork(LaundryDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // NOTA: Con EnableRetryOnFailure activo, no podemos usar BeginTransactionAsync.
        // EF Core maneja las transacciones automáticamente en SaveChangesAsync.
        // Este método se mantiene por compatibilidad pero no hace nada.
        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        // NOTA: No hay transacción explícita que confirmar.
        // SaveChangesAsync ya confirmó los cambios automáticamente.
        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        // NOTA: No hay transacción explícita que revertir.
        // Si SaveChangesAsync falla, EF Core revierte automáticamente.
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // SaveChangesAsync maneja su propia transacción con retry automático
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public IDbConnection GetConnection()
    {
        var connection = _context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        return connection;
    }

    public IDbTransaction? GetTransaction()
    {
        // Con EnableRetryOnFailure no hay transacción manual disponible
        return null;
    }

    public void Dispose()
    {
        // No hay transacción que disponer
    }
}
