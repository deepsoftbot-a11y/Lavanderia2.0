using LaundryManagement.Application.DTOs.Orders;
using MediatR;

namespace LaundryManagement.Application.Queries.Orders;

public record SearchOrdersQuery(string Query) : IRequest<List<OrderSummaryDto>>;
