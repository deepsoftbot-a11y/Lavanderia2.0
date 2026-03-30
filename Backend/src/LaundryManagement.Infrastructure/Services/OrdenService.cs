using System.Data.Common;
using System.Text.Json;
using LaundryManagement.Application.DTOs.Ordenes;
using LaundryManagement.Application.Interfaces;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Infrastructure.Persistence;
using LaundryManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaundryManagement.Infrastructure.Services;

public class OrdenService : IOrdenService
{
    private readonly LaundryDbContext _context;

    public OrdenService(LaundryDbContext context)
    {
        _context = context;
    }

    public async Task<CrearOrdenResponse> CrearOrdenAsync(CrearOrdenRequest request)
    {
        try
        {
            var detalles = JsonSerializer.Deserialize<List<OrdenDetalleJson>>(
                request.DetalleJSON,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new Domain.Exceptions.ValidationException("El detalle de la orden no puede estar vacío");

            var folio = await GenerateNextFolioAsync();
            var subtotal = detalles.Sum(d => d.PrecioUnitario * (d.Cantidad ?? 1));

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var orden = new Ordene
            {
                FolioOrden      = folio,
                ClienteId       = request.ClienteID,
                FechaPrometida  = request.FechaPrometida,
                RecibidoPor     = request.RecibidoPor,
                EstadoOrdenId   = 1,
                Subtotal        = subtotal,
                Descuento       = 0,
                Total           = subtotal,
                Observaciones   = request.Observaciones,
                Ubicaciones     = request.Ubicaciones
            };

            _context.Ordenes.Add(orden);
            await _context.SaveChangesAsync();

            for (int i = 0; i < detalles.Count; i++)
            {
                var d = detalles[i];
                var lineTotal = d.PrecioUnitario * (d.Cantidad ?? 1);
                _context.OrdenesDetalles.Add(new OrdenesDetalle
                {
                    OrdenId        = orden.OrdenId,
                    NumeroLinea    = i + 1,
                    ServicioId     = d.ServicioID,
                    ServicioPrendaId = d.ServicioPrendaID,
                    Cantidad       = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    PesoKilos      = d.PesoKilos,
                    Subtotal       = lineTotal,
                    DescuentoLinea = 0,
                    TotalLinea     = lineTotal,
                    Observaciones  = d.Observaciones
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new CrearOrdenResponse { OrdenID = orden.OrdenId };
        }
        catch (Domain.Exceptions.ValidationException) { throw; }
        catch (DbException ex)
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
            var orden = await _context.Ordenes.FindAsync(request.OrdenID)
                ?? throw new NotFoundException($"Orden {request.OrdenID} no encontrada");

            orden.EstadoOrdenId = request.NuevoEstadoID;

            _context.HistorialEstadosOrdens.Add(new HistorialEstadosOrden
            {
                OrdenId       = request.OrdenID,
                EstadoOrdenId = request.NuevoEstadoID,
                FechaCambio   = DateTime.UtcNow,
                CambiadoPor   = request.CambiadoPor,
                Comentarios   = request.Comentarios
            });

            await _context.SaveChangesAsync();
        }
        catch (NotFoundException) { throw; }
        catch (DbException ex)
        {
            throw new DatabaseException("Error al cambiar el estado de la orden en la base de datos", ex);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            throw new DatabaseException("Error inesperado al cambiar el estado de la orden", ex);
        }
    }

    private async Task<string> GenerateNextFolioAsync()
    {
        var datePrefix = DateTime.Today.ToString("yyyyMMdd");
        var pattern    = $"ORD-{datePrefix}-%";

        var lastOrder = await _context.Ordenes
            .Where(o => EF.Functions.Like(o.FolioOrden, pattern))
            .OrderByDescending(o => o.FolioOrden)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastOrder != null)
        {
            var lastFolio = lastOrder.FolioOrden;
            if (lastFolio.Length >= 4 && int.TryParse(lastFolio[^4..], out int n))
                nextNumber = n + 1;
        }

        return $"ORD-{datePrefix}-{nextNumber:D4}";
    }

    private sealed class OrdenDetalleJson
    {
        public int ServicioID { get; set; }
        public int? ServicioPrendaID { get; set; }
        public int? Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal? PesoKilos { get; set; }
        public string? Observaciones { get; set; }
    }
}
