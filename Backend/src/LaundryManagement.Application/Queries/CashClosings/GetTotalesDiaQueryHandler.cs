using LaundryManagement.Domain.Repositories;
using MediatR;

namespace LaundryManagement.Application.Queries.CashClosings;

/// <summary>
/// Handler para obtener los totales de pagos del día
/// Usa el repositorio de dominio para mantener arquitectura DDD pura
/// </summary>
public sealed class GetTotalesDiaQueryHandler : IRequestHandler<GetTotalesDiaQuery, TotalesDiaDto>
{
    private readonly ICashClosingRepository _repository;

    public GetTotalesDiaQueryHandler(ICashClosingRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<TotalesDiaDto> Handle(GetTotalesDiaQuery query, CancellationToken cancellationToken)
    {
        // Obtener los totales desde el repositorio (que usa el stored procedure internamente)
        var dayTotals = await _repository.GetDayTotalsAsync(query.Fecha, query.CajeroId, cancellationToken);

        // Mapear Value Object de dominio a DTO de aplicación
        return new TotalesDiaDto
        {
            TotalEfectivo = dayTotals.TotalCash.Amount,
            TotalTarjeta = dayTotals.TotalCard.Amount,
            TotalTransferencia = dayTotals.TotalTransfer.Amount,
            TotalOtros = dayTotals.TotalOther.Amount,
            TotalPagado = dayTotals.TotalPaid.Amount
        };
    }
}
