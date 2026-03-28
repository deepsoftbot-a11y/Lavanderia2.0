using MediatR;

namespace LaundryManagement.Application.Queries.Orders;

public record GetOrdersQuery(
    string? Search,
    int? ClientId,
    DateTime? StartDate,
    DateTime? EndDate,
    string SortBy = "createdAt",
    string SortOrder = "desc"
) : IRequest<List<OrderResponseDto>>;
