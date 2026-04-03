namespace LaundryManagement.Application.Common;

public record PagedResult<T>(
    List<T> Data,
    int TotalCount,
    int Page,
    int PageSize
);
