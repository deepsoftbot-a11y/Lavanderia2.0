using LaundryManagement.Application.Common;
using MediatR;

namespace LaundryManagement.Application.Queries.Orders;

public record GetOrdersQuery(
    string? Search,
    int? ClientId,
    DateTime? StartDate,
    DateTime? EndDate,
    int[]? StatusIds,
    string[]? PaymentStatuses,
    string SortBy = "createdAt",
    string SortOrder = "desc",
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<OrderResponseDto>>;
