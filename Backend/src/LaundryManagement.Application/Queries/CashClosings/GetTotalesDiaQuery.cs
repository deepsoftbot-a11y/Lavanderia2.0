using MediatR;

namespace LaundryManagement.Application.Queries.CashClosings;

/// <summary>
/// Query para obtener los totales de pagos del día para un cajero específico
/// </summary>
public sealed record GetTotalesDiaQuery(DateTime Fecha, int CajeroId) : IRequest<TotalesDiaDto>;
